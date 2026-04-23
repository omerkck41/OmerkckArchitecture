using Microsoft.Extensions.Logging;

namespace Kck.EventBus.InMemory;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Error handling {EventType} in {HandlerType}")]
    public static partial void HandlerError(ILogger logger, Exception exception, string eventType, string handlerType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error handling {EventType}")]
    public static partial void DispatchError(ILogger logger, Exception exception, string eventType);
}
