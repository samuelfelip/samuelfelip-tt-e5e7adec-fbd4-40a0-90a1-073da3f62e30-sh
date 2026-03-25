namespace HighPerformance.Ingest.Application.DTOs;

/// <summary>Aggregated score for a single user in the current time window.</summary>
/// <param name="UserId">User identifier.</param>
/// <param name="TotalScore">Sum of scores in the window.</param>
public sealed record UserScoreDto(string UserId, long TotalScore);
