using FluentValidation;

namespace HighPerformance.Ingest.Application.Commands;

public sealed class RegisterScoreCommandValidator : AbstractValidator<RegisterScoreCommand>
{
    public RegisterScoreCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Timestamp)
            .Must(ts => !ts.HasValue || ts.Value.Kind != DateTimeKind.Unspecified)
            .WithMessage("Timestamp must include timezone information (ISO-8601 UTC or offset).");
    }
}
