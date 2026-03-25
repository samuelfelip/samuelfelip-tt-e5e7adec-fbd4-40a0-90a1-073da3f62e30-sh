namespace HighPerformance.Ingest.Application.Settings;

public sealed class LeaderboardSettings
{
    public const string SectionName = "LeaderboardSettings";
    public int WindowDays { get; set; } = 7;
    public int MaxScoreBatchSize { get; set; } = 10_000;
}
