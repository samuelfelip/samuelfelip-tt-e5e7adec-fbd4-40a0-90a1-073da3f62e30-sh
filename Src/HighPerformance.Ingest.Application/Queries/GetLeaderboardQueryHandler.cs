using HighPerformance.Ingest.Application.Abstractions;
using HighPerformance.Ingest.Application.DTOs;
using HighPerformance.Ingest.Application.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HighPerformance.Ingest.Application.Queries;

public sealed class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, IReadOnlyList<LeaderboardEntryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly LeaderboardSettings _settings;

    public GetLeaderboardQueryHandler(IApplicationDbContext dbContext, IOptions<LeaderboardSettings> settings)
    {
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-_settings.WindowDays);
        var top = request.Top > 0 ? request.Top : 10;

        var entries = await _dbContext.ScoreEntries
            .Where(s => s.Timestamp >= cutoff)
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, TotalScore = g.Sum(s => s.Score) })
            .OrderByDescending(x => x.TotalScore)
            .Take(top)
            .ToListAsync(cancellationToken);

        return entries
            .Select((e, i) => new LeaderboardEntryDto(e.UserId, e.TotalScore, i + 1))
            .ToList()
            .AsReadOnly();
    }
}
