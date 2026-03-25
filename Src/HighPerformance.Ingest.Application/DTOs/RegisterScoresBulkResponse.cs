namespace HighPerformance.Ingest.Application.DTOs;

/// <summary>Result of a successful bulk score registration.</summary>
/// <param name="Count">Number of rows written (same as request size when successful).</param>
/// <param name="EntryIds">Identifiers in the same order as the submitted <c>entries</c> array.</param>
public sealed record RegisterScoresBulkResponse(int Count, IReadOnlyList<Guid> EntryIds);
