namespace HighPerformance.Ingest.Application.DTOs;

/// <summary>Body for <c>POST /api/scores</c>.</summary>
/// <param name="UserId">Logical player or account id (required).</param>
/// <param name="Score">Non-negative points to add for this event.</param>
/// <param name="Timestamp">Optional instant; must include timezone (ISO-8601 <c>Z</c> or offset). Omit to use server UTC now.</param>
public sealed record RegisterScoreRequest(string UserId, long Score, DateTime? Timestamp);
