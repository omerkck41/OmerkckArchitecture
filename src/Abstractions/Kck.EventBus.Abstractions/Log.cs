using Microsoft.Extensions.Logging;

namespace Kck.EventBus.Abstractions;

/// <summary>
/// Provides high-performance structured log messages for the EventBus abstractions layer using source-generated <see cref="LoggerMessage"/> delegates.
/// </summary>
internal static partial class Log
{
    /// <summary>Logs a warning when a deserialized message does not implement <c>IntegrationEvent</c> and will be skipped.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "Deserialized message for {EventName} is not an IntegrationEvent, skipping")]
    public static partial void NotAnIntegrationEvent(ILogger logger, string eventName);

    /// <summary>Logs a warning when no DI-registered handler is found for the given handler type while processing an event.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "No handler registered in DI for {HandlerType} while processing {EventName}")]
    public static partial void NoHandlerRegistered(ILogger logger, string handlerType, string eventName);
}
