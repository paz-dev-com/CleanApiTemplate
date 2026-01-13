using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CleanApiTemplate.Application.Behaviors;

/// <summary>
/// Pipeline behavior for logging performance of commands/queries
/// Demonstrates performance monitoring and logging
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger = logger;
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        TResponse? response = await next(cancellationToken);

        _timer.Stop();

        long elapsedMilliseconds = _timer.ElapsedMilliseconds;

        // Log warning if request takes longer than 500ms
        if (elapsedMilliseconds > 500)
        {
            string requestName = typeof(TRequest).Name;

            _logger.LogWarning(
                "Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds) {@Request}",
                requestName, elapsedMilliseconds, request);
        }

        return response;
    }
}
