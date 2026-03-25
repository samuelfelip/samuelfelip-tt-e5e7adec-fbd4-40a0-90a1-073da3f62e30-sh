using MediatR;
using Microsoft.Extensions.Logging;

namespace HighPerformance.Ingest.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;
        _logger.LogInformation("Handling request {RequestName} at {StartedAt}", typeof(TRequest).Name, startedAt);

        var response = await next();

        var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
        _logger.LogInformation("Handled request {RequestName} in {ElapsedMs}ms", typeof(TRequest).Name, elapsedMs);

        return response;
    }
}
