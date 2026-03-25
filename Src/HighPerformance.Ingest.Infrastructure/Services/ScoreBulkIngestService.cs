using HighPerformance.Ingest.Application.Abstractions;
using HighPerformance.Ingest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace HighPerformance.Ingest.Infrastructure.Services;

public sealed class ScoreBulkIngestService : IScoreBulkIngestService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public ScoreBulkIngestService(IApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<int> InsertScoreEntriesAsync(IReadOnlyCollection<ScoreEntry> entries, CancellationToken cancellationToken)
    {
        if (entries.Count == 0)
            return 0;

        var isNpgsql = _dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;
        if (isNpgsql)
            return await InsertViaNpgsqlBinaryCopyAsync(entries, cancellationToken);

        foreach (var entry in entries)
            _dbContext.ScoreEntries.Add(entry);
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> InsertViaNpgsqlBinaryCopyAsync(IReadOnlyCollection<ScoreEntry> entries, CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' is missing.");

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var writer = await connection.BeginBinaryImportAsync(
            "COPY \"ScoreEntries\" (\"Id\", \"UserId\", \"Score\", \"Timestamp\") FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        foreach (var entry in entries)
        {
            var ts = entry.Timestamp.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(entry.Timestamp, DateTimeKind.Utc)
                : entry.Timestamp.ToUniversalTime();

            await writer.StartRowAsync(cancellationToken);
            await writer.WriteAsync(entry.Id, cancellationToken);
            await writer.WriteAsync(entry.UserId, cancellationToken);
            await writer.WriteAsync(entry.Score, cancellationToken);
            await writer.WriteAsync(ts, cancellationToken);
        }

        await writer.CompleteAsync(cancellationToken);
        return entries.Count;
    }
}
