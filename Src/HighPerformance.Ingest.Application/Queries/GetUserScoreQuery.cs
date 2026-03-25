using HighPerformance.Ingest.Application.DTOs;
using MediatR;

namespace HighPerformance.Ingest.Application.Queries;

public sealed record GetUserScoreQuery(string UserId) : IRequest<UserScoreDto?>;
