using HighPerformance.Ingest.Application.DTOs;
using MediatR;

namespace HighPerformance.Ingest.Application.Queries;

public sealed record GetLeaderboardQuery(int Top) : IRequest<IReadOnlyList<LeaderboardEntryDto>>;
