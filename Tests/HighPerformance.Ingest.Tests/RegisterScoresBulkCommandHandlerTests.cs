using HighPerformance.Ingest.Application.Abstractions;
using HighPerformance.Ingest.Application.Commands;
using HighPerformance.Ingest.Application.DTOs;
using HighPerformance.Ingest.Domain.Entities;
using HighPerformance.Ingest.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HighPerformance.Ingest.Tests;

public sealed class RegisterScoresBulkCommandHandlerTests
{
    private sealed class EfScoreBulkIngest : IScoreBulkIngestService
    {
        private readonly IApplicationDbContext _db;

        public EfScoreBulkIngest(IApplicationDbContext db) => _db = db;

        public async Task<int> InsertScoreEntriesAsync(
            IReadOnlyCollection<ScoreEntry> entries,
            CancellationToken cancellationToken)
        {
            foreach (var e in entries)
                _db.ScoreEntries.Add(e);
            return await _db.SaveChangesAsync(cancellationToken);
        }
    }

    [Fact]
    public async Task Handle_Persists_All_Rows_In_One_Batch()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var handler = new RegisterScoresBulkCommandHandler(new EfScoreBulkIngest(dbContext));
        var now = DateTime.UtcNow;

        var result = await handler.Handle(
            new RegisterScoresBulkCommand(
            [
                new RegisterScoreRequest("u1", 10, now),
                new RegisterScoreRequest("u2", 20, now),
                new RegisterScoreRequest("u1", 5, null)
            ]),
            CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.EntryIds.Count);
        Assert.All(result.EntryIds, id => Assert.NotEqual(Guid.Empty, id));

        var stored = await dbContext.ScoreEntries.AsNoTracking().OrderBy(e => e.UserId).ThenBy(e => e.Score).ToListAsync();
        Assert.Equal(3, stored.Count);
        Assert.Equal(15, stored.Where(e => e.UserId == "u1").Sum(e => e.Score));
        Assert.Equal(20, stored.Single(e => e.UserId == "u2").Score);
    }
}
