using System.Diagnostics;
using Kck.Core.Abstractions.Pipeline;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.MediatR.Behaviors;

/// <summary>
/// Logs request execution timing for requests implementing <see cref="ILoggableRequest"/>.
/// Emits a warning when execution exceeds 500ms.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ILoggableRequest
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        Log.HandlingRequest(logger, requestName);

        var sw = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        sw.Stop();

        if (sw.ElapsedMilliseconds > 500)
        {
            Log.LongRunningRequest(logger, requestName, sw.ElapsedMilliseconds);
        }
        else
        {
            Log.HandledRequest(logger, requestName, sw.ElapsedMilliseconds);
        }

        return response;
    }
}
