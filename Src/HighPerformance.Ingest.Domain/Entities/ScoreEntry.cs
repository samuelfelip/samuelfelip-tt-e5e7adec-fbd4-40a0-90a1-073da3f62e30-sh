namespace HighPerformance.Ingest.Domain.Entities;

public sealed class ScoreEntry
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public long Score { get; set; }
    public DateTime Timestamp { get; set; }

    // PostgreSQL xmin optimistic concurrency token.
    public uint Version { get; set; }
}
