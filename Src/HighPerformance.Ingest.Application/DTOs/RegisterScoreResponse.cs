namespace HighPerformance.Ingest.Application.DTOs;

/// <summary>Result of a successful single score registration.</summary>
/// <param name="EntryId">Primary key of the new <c>ScoreEntries</c> row.</param>
public sealed record RegisterScoreResponse(Guid EntryId);
