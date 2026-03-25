namespace HighPerformance.Ingest.Application.DTOs;

/// <summary>Body for <c>POST /api/scores/bulk</c>.</summary>
/// <param name="Entries">Score lines to append; length must be between 1 and <c>LeaderboardSettings:MaxScoreBatchSize</c>.</param>
public sealed record RegisterScoresBulkRequest(IReadOnlyList<RegisterScoreRequest> Entries);
