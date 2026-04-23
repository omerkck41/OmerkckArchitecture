using Microsoft.Extensions.Logging;

namespace Kck.EventBus.Abstractions;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Deserialized message for {EventName} is not an IntegrationEvent, skipping")]
    public static partial void NotAnIntegrationEvent(ILogger logger, string eventName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No handler registered in DI for {HandlerType} while processing {EventName}")]
    public static partial void NoHandlerRegistered(ILogger logger, string handlerType, string eventName);
}
