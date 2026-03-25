using HighPerformance.Ingest.Application.Commands;
using HighPerformance.Ingest.Application.DTOs;
using HighPerformance.Ingest.Application.Settings;
using Microsoft.Extensions.Options;

namespace HighPerformance.Ingest.Tests;

public sealed class RegisterScoreTimestampValidationTests
{
    [Fact]
    public void RegisterScoreValidator_Fails_WhenTimestampHasUnspecifiedKind()
    {
        var validator = new RegisterScoreCommandValidator();
        var cmd = new RegisterScoreCommand(
            "u1",
            10,
            new DateTime(2026, 3, 25, 12, 0, 0, DateTimeKind.Unspecified));

        var result = validator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Timestamp");
    }

    [Fact]
    public void RegisterScoreValidator_Passes_WhenTimestampIsUtc()
    {
        var validator = new RegisterScoreCommandValidator();
        var cmd = new RegisterScoreCommand("u1", 10, DateTime.UtcNow);

        var result = validator.Validate(cmd);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterScoresBulkValidator_Fails_WhenAnyTimestampHasUnspecifiedKind()
    {
        var validator = new RegisterScoresBulkCommandValidator(
            Options.Create(new LeaderboardSettings { MaxScoreBatchSize = 10 }));

        var cmd = new RegisterScoresBulkCommand(
        [
            new RegisterScoreRequest("u1", 10, DateTime.UtcNow),
            new RegisterScoreRequest("u2", 20, new DateTime(2026, 3, 25, 12, 0, 0, DateTimeKind.Unspecified))
        ]);

        var result = validator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Timestamp", StringComparison.Ordinal));
    }
}
