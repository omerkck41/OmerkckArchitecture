using Microsoft.Extensions.Logging;

namespace Kck.EventBus.InMemory;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Error handling {EventType} in {HandlerType}")]
    public static partial void HandlerError(ILogger logger, Exception exception, string eventType, string handlerType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Handler {HandlerType} is subscribed to {EventType} but not registered in DI; it was skipped")]
    public static partial void SubscribedHandlerNotRegistered(ILogger logger, string handlerType, string eventType);
}
