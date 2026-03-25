using FluentValidation;
using HighPerformance.Ingest.Application.Settings;
using Microsoft.Extensions.Options;

namespace HighPerformance.Ingest.Application.Commands;

public sealed class RegisterScoresBulkCommandValidator : AbstractValidator<RegisterScoresBulkCommand>
{
    public RegisterScoresBulkCommandValidator(IOptions<LeaderboardSettings> settings)
    {
        var max = settings.Value.MaxScoreBatchSize;

        RuleFor(x => x.Entries)
            .NotNull()
            .NotEmpty()
            .Must(e => e.Count <= max)
            .WithMessage($"At most {max} score rows per request.");

        RuleForEach(x => x.Entries!).ChildRules(entry =>
        {
            entry.RuleFor(e => e.UserId).NotEmpty();
            entry.RuleFor(e => e.Score).GreaterThanOrEqualTo(0);
            entry.RuleFor(e => e.Timestamp)
                .Must(ts => !ts.HasValue || ts.Value.Kind != DateTimeKind.Unspecified)
                .WithMessage("Timestamp must include timezone information (ISO-8601 UTC or offset).");
        });
    }
}
