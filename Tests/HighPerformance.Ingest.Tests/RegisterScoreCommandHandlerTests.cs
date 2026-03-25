using HighPerformance.Ingest.Application.Abstractions;
using HighPerformance.Ingest.Application.Commands;
using HighPerformance.Ingest.Domain.Entities;
using HighPerformance.Ingest.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HighPerformance.Ingest.Tests;

public sealed class RegisterScoreCommandHandlerTests
{
    private sealed class EfScoreBulkIngest : IScoreBulkIngestService
    {
        private readonly IApplicationDbContext _db;

        public EfScoreBulkIngest(IApplicationDbContext db) => _db = db;

        public async Task<int> InsertScoreEntriesAsync(IReadOnlyCollection<ScoreEntry> entries, CancellationToken cancellationToken)
        {
            foreach (var e in entries)
                _db.ScoreEntries.Add(e);
            return await _db.SaveChangesAsync(cancellationToken);
        }
    }

    [Fact]
    public async Task Handle_InsertsScoreEntry_ReturnsEntryId()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var handler = new RegisterScoreCommandHandler(new EfScoreBulkIngest(dbContext));
        var command = new RegisterScoreCommand("user-1", 150, DateTime.UtcNow);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.EntryId);

        var entry = await dbContext.ScoreEntries.SingleAsync(e => e.Id == result.EntryId);
        Assert.Equal("user-1", entry.UserId);
        Assert.Equal(150, entry.Score);
    }

    [Fact]
    public async Task Handle_DefaultsTimestamp_WhenNull()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var handler = new RegisterScoreCommandHandler(new EfScoreBulkIngest(dbContext));
        var before = DateTime.UtcNow;
        var result = await handler.Handle(new RegisterScoreCommand("user-2", 50, null), CancellationToken.None);
        var after = DateTime.UtcNow;

        var entry = await dbContext.ScoreEntries.SingleAsync(e => e.Id == result.EntryId);
        Assert.InRange(entry.Timestamp, before, after);
    }

    [Fact]
    public async Task Handle_NormalizesTimestamp_ToUtc()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var handler = new RegisterScoreCommandHandler(new EfScoreBulkIngest(dbContext));
        var localTs = DateTime.SpecifyKind(new DateTime(2026, 3, 25, 12, 0, 0), DateTimeKind.Local);

        var result = await handler.Handle(new RegisterScoreCommand("user-3", 80, localTs), CancellationToken.None);

        var entry = await dbContext.ScoreEntries.SingleAsync(e => e.Id == result.EntryId);
        Assert.Equal(DateTimeKind.Utc, entry.Timestamp.Kind);
    }
}
