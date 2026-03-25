using HighPerformance.Ingest.Application.DTOs;
using MediatR;

namespace HighPerformance.Ingest.Application.Commands;

public sealed record RegisterScoresBulkCommand(IReadOnlyList<RegisterScoreRequest> Entries) : IRequest<RegisterScoresBulkResponse>;
