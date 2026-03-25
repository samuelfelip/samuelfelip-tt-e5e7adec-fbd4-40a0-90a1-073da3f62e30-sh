using HighPerformance.Ingest.Application.Abstractions;
using HighPerformance.Ingest.Application.DTOs;
using HighPerformance.Ingest.Application.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HighPerformance.Ingest.Application.Queries;

public sealed class GetUserScoreQueryHandler : IRequestHandler<GetUserScoreQuery, UserScoreDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly LeaderboardSettings _settings;

    public GetUserScoreQueryHandler(IApplicationDbContext dbContext, IOptions<LeaderboardSettings> settings)
    {
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    public async Task<UserScoreDto?> Handle(GetUserScoreQuery request, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-_settings.WindowDays);

        var result = await _dbContext.ScoreEntries
            .Where(s => s.UserId == request.UserId && s.Timestamp >= cutoff)
            .GroupBy(s => s.UserId)
            .Select(g => new UserScoreDto(g.Key, g.Sum(s => s.Score)))
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}
