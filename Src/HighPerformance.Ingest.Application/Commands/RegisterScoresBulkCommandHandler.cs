using HighPerformance.Ingest.Application.Abstractions;
using HighPerformance.Ingest.Application.DTOs;
using HighPerformance.Ingest.Domain.Entities;
using MediatR;

namespace HighPerformance.Ingest.Application.Commands;

public sealed class RegisterScoresBulkCommandHandler : IRequestHandler<RegisterScoresBulkCommand, RegisterScoresBulkResponse>
{
    private readonly IScoreBulkIngestService _scoreBulkIngest;

    public RegisterScoresBulkCommandHandler(IScoreBulkIngestService scoreBulkIngest)
    {
        _scoreBulkIngest = scoreBulkIngest;
    }

    public async Task<RegisterScoresBulkResponse> Handle(RegisterScoresBulkCommand request, CancellationToken cancellationToken)
    {
        var entries = new List<ScoreEntry>(request.Entries.Count);
        var ids = new List<Guid>(request.Entries.Count);

        foreach (var line in request.Entries)
        {
            var id = Guid.NewGuid();
            ids.Add(id);
            entries.Add(new ScoreEntry
            {
                Id = id,
                UserId = line.UserId,
                Score = line.Score,
                Timestamp = line.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow
            });
        }

        var inserted = await _scoreBulkIngest.InsertScoreEntriesAsync(entries, cancellationToken);
        return new RegisterScoresBulkResponse(inserted, ids);
    }
}
