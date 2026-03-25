using HighPerformance.Ingest.Application.Abstractions;
using HighPerformance.Ingest.Application.DTOs;
using HighPerformance.Ingest.Domain.Entities;
using MediatR;

namespace HighPerformance.Ingest.Application.Commands;

public sealed class RegisterScoreCommandHandler : IRequestHandler<RegisterScoreCommand, RegisterScoreResponse>
{
    private readonly IScoreBulkIngestService _scoreBulkIngest;

    public RegisterScoreCommandHandler(IScoreBulkIngestService scoreBulkIngest)
    {
        _scoreBulkIngest = scoreBulkIngest;
    }

    public async Task<RegisterScoreResponse> Handle(RegisterScoreCommand request, CancellationToken cancellationToken)
    {
        var entry = new ScoreEntry
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Score = request.Score,
            Timestamp = request.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow
        };

        await _scoreBulkIngest.InsertScoreEntriesAsync([entry], cancellationToken);

        return new RegisterScoreResponse(entry.Id);
    }
}
