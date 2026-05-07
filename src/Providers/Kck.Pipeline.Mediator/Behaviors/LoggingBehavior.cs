using System.Diagnostics;
using Kck.Core.Abstractions.Pipeline;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Logs request execution timing for requests implementing <see cref="ILoggableRequest"/>.
/// Emits a warning when execution exceeds 500ms.
/// </summary>
public sealed class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IRequest<TResponse>, ILoggableRequest
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;
        Log.HandlingRequest(logger, messageName);

        var sw = Stopwatch.StartNew();
        var response = await next(message, cancellationToken).ConfigureAwait(false);
        sw.Stop();

        if (sw.ElapsedMilliseconds > 500)
            Log.LongRunningRequest(logger, messageName, sw.ElapsedMilliseconds);
        else
            Log.HandledRequest(logger, messageName, sw.ElapsedMilliseconds);

        return response;
    }
}
