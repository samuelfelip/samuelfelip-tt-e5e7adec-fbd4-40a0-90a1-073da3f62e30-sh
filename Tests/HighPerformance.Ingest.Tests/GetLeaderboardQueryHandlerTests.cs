using HighPerformance.Ingest.Application.Queries;
using HighPerformance.Ingest.Application.Settings;
using HighPerformance.Ingest.Domain.Entities;
using HighPerformance.Ingest.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HighPerformance.Ingest.Tests;

public sealed class GetLeaderboardQueryHandlerTests
{
    private static IOptions<LeaderboardSettings> DefaultSettings()
        => Options.Create(new LeaderboardSettings { WindowDays = 7 });

    [Fact]
    public async Task Handle_ReturnsRankedUsersInDescendingOrder()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var now = DateTime.UtcNow;
        dbContext.ScoreEntries.AddRange(
            new ScoreEntry { Id = Guid.NewGuid(), UserId = "alice", Score = 100, Timestamp = now },
            new ScoreEntry { Id = Guid.NewGuid(), UserId = "alice", Score = 50, Timestamp = now },
            new ScoreEntry { Id = Guid.NewGuid(), UserId = "bob", Score = 200, Timestamp = now },
            new ScoreEntry { Id = Guid.NewGuid(), UserId = "carol", Score = 120, Timestamp = now }
        );
        await dbContext.SaveChangesAsync();

        var handler = new GetLeaderboardQueryHandler(dbContext, DefaultSettings());
        var result = await handler.Handle(new GetLeaderboardQuery(10), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("bob", result[0].UserId);
        Assert.Equal(200, result[0].TotalScore);
        Assert.Equal(1, result[0].Rank);
        Assert.Equal("alice", result[1].UserId);
        Assert.Equal(150, result[1].TotalScore);
        Assert.Equal(2, result[1].Rank);
        Assert.Equal("carol", result[2].UserId);
        Assert.Equal(3, result[2].Rank);
    }

    [Fact]
    public async Task Handle_ExcludesScoresOutsideWindow()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var now = DateTime.UtcNow;
        dbContext.ScoreEntries.AddRange(
            new ScoreEntry { Id = Guid.NewGuid(), UserId = "alice", Score = 100, Timestamp = now },
            new ScoreEntry { Id = Guid.NewGuid(), UserId = "alice", Score = 500, Timestamp = now.AddDays(-30) }
        );
        await dbContext.SaveChangesAsync();

        var handler = new GetLeaderboardQueryHandler(dbContext, DefaultSettings());
        var result = await handler.Handle(new GetLeaderboardQuery(10), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(100, result[0].TotalScore);
    }

    [Fact]
    public async Task Handle_RespectsTopLimit()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var now = DateTime.UtcNow;
        for (var i = 0; i < 20; i++)
        {
            dbContext.ScoreEntries.Add(new ScoreEntry
            {
                Id = Guid.NewGuid(),
                UserId = $"user-{i}",
                Score = 100 + i,
                Timestamp = now
            });
        }
        await dbContext.SaveChangesAsync();

        var handler = new GetLeaderboardQueryHandler(dbContext, DefaultSettings());
        var result = await handler.Handle(new GetLeaderboardQuery(5), CancellationToken.None);

        Assert.Equal(5, result.Count);
    }
}
