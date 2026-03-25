using HighPerformance.Ingest.Application.DTOs;
using MediatR;

namespace HighPerformance.Ingest.Application.Commands;

public sealed record RegisterScoreCommand(string UserId, long Score, DateTime? Timestamp) : IRequest<RegisterScoreResponse>;
