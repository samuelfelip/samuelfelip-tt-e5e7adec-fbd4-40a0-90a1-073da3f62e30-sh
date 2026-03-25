using HighPerformance.Ingest.Domain.Entities;

namespace HighPerformance.Ingest.Application.Abstractions;

public interface IScoreBulkIngestService
{
    Task<int> InsertScoreEntriesAsync(IReadOnlyCollection<ScoreEntry> entries, CancellationToken cancellationToken);
}
