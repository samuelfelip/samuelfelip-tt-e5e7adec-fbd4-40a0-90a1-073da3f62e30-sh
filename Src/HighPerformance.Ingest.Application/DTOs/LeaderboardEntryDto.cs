namespace HighPerformance.Ingest.Application.DTOs;

/// <summary>One row in the global leaderboard for the current time window.</summary>
/// <param name="UserId">User identifier.</param>
/// <param name="TotalScore">Sum of scores in the window.</param>
/// <param name="Rank">1-based position (1 = highest total).</param>
public sealed record LeaderboardEntryDto(string UserId, long TotalScore, int Rank);
