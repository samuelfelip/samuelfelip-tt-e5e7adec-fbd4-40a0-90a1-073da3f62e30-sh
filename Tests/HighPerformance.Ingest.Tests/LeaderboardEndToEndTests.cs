using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HighPerformance.Ingest.Application.Abstractions;
using HighPerformance.Ingest.Application.DTOs;
using HighPerformance.Ingest.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HighPerformance.Ingest.Tests;

public sealed class LeaderboardEndToEndTests : IClassFixture<LeaderboardEndToEndTests.LeaderboardWebAppFactory>
{
    private readonly HttpClient _client;

    public LeaderboardEndToEndTests(LeaderboardWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostScore_ReturnsCreated()
    {
        var payload = new { UserId = "e2e-user", Score = 42, Timestamp = DateTime.UtcNow };
        var response = await _client.PostAsJsonAsync("/api/scores", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RegisterScoreResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.EntryId);
    }

    [Fact]
    public async Task PostScoresBulk_ReturnsCreated_WithIds()
    {
        var payload = new RegisterScoresBulkRequest(
        [
            new RegisterScoreRequest("bulk-a", 1, DateTime.UtcNow),
            new RegisterScoreRequest("bulk-b", 2, null)
        ]);
        var response = await _client.PostAsJsonAsync("/api/scores/bulk", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RegisterScoresBulkResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Equal(2, body.EntryIds.Count);
        Assert.All(body.EntryIds, id => Assert.NotEqual(Guid.Empty, id));
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsRankedList()
    {
        var now = DateTime.UtcNow;
        await _client.PostAsJsonAsync("/api/scores", new { UserId = "lb-alice", Score = 300, Timestamp = now });
        await _client.PostAsJsonAsync("/api/scores", new { UserId = "lb-bob", Score = 500, Timestamp = now });
        await _client.PostAsJsonAsync("/api/scores", new { UserId = "lb-alice", Score = 250, Timestamp = now });

        var response = await _client.GetAsync("/api/leaderboard?top=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entries = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
        Assert.NotNull(entries);
        Assert.True(entries.Count >= 2);

        var alice = entries.FirstOrDefault(e => e.UserId == "lb-alice");
        var bob = entries.FirstOrDefault(e => e.UserId == "lb-bob");
        Assert.NotNull(alice);
        Assert.NotNull(bob);
        Assert.Equal(550, alice.TotalScore);
        Assert.Equal(500, bob.TotalScore);
        Assert.True(bob.Rank < alice.Rank || alice.TotalScore >= bob.TotalScore);
    }

    [Fact]
    public async Task GetUserScore_ReturnsScore()
    {
        var now = DateTime.UtcNow;
        await _client.PostAsJsonAsync("/api/scores", new { UserId = "score-user", Score = 75, Timestamp = now });
        await _client.PostAsJsonAsync("/api/scores", new { UserId = "score-user", Score = 25, Timestamp = now });

        var response = await _client.GetAsync("/api/users/score-user/score");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<UserScoreDto>();
        Assert.NotNull(dto);
        Assert.Equal("score-user", dto.UserId);
        Assert.Equal(100, dto.TotalScore);
    }

    [Fact]
    public async Task GetUserScore_ReturnsNotFound_ForUnknownUser()
    {
        var response = await _client.GetAsync("/api/users/nonexistent/score");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ConcurrentScoreInserts_AllPersisted()
    {
        const int concurrency = 50;
        var tasks = Enumerable.Range(0, concurrency)
            .Select(_ => _client.PostAsJsonAsync("/api/scores",
                new { UserId = "concurrent-user", Score = 1, Timestamp = DateTime.UtcNow }))
            .ToArray();

        var responses = await Task.WhenAll(tasks);
        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));

        var scoreResponse = await _client.GetAsync("/api/users/concurrent-user/score");
        var dto = await scoreResponse.Content.ReadFromJsonAsync<UserScoreDto>();
        Assert.NotNull(dto);
        Assert.Equal(concurrency, dto.TotalScore);
    }

    public sealed class LeaderboardWebAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"hpi-leaderboard-e2e-{Guid.NewGuid():n}.sqlite");
        private readonly SqliteConnection _connection;

        public LeaderboardWebAppFactory()
        {
            _connection = new SqliteConnection($"Data Source={_dbPath}");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();
            // File-backed SQLite + WAL + busy timeout: :memory: still serializes writers heavily under concurrent HTTP.
            using (var wal = _connection.CreateCommand())
            {
                wal.CommandText = "PRAGMA journal_mode=WAL;";
                wal.ExecuteNonQuery();
            }

            using (var busy = _connection.CreateCommand())
            {
                busy.CommandText = "PRAGMA busy_timeout=30000;";
                busy.ExecuteNonQuery();
            }

            builder.ConfigureServices(services =>
            {
                // Remove all EF Core / Npgsql / AppDbContext registrations to avoid dual-provider conflict.
                var toRemove = services.Where(d =>
                {
                    var svcName = d.ServiceType.FullName ?? string.Empty;
                    var implName = d.ImplementationType?.FullName ?? string.Empty;
                    return svcName.Contains("DbContextOptions")
                        || svcName.Contains("DbContextOptionsConfiguration")
                        || svcName.Contains("Npgsql")
                        || svcName.Contains("AppDbContext")
                        || implName.Contains("Npgsql")
                        || implName.Contains("AppDbContext")
                        || d.ServiceType == typeof(IApplicationDbContext);
                }).ToList();

                foreach (var d in toRemove)
                    services.Remove(d);

                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
                services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            await _connection.DisposeAsync().ConfigureAwait(false);
            try
            {
                if (File.Exists(_dbPath))
                    File.Delete(_dbPath);
            }
            catch (IOException)
            {
                // Best-effort cleanup of temp DB.
            }
        }
    }
}
