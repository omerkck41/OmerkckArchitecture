using Microsoft.Extensions.Logging;

namespace Kck.EventBus.RabbitMq;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Published {EventName} with Id {EventId}")]
    public static partial void Published(ILogger logger, string eventName, Guid eventId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error processing {EventName}")]
    public static partial void ProcessingError(ILogger logger, Exception exception, string eventName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Message {MessageId} exceeded max delivery count, dead-lettered")]
    public static partial void MessageDeadLettered(ILogger logger, string? messageId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Subscribed to {EventName} on queue {QueueName}")]
    public static partial void Subscribed(ILogger logger, string eventName, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Connected to RabbitMQ at {Host}:{Port}")]
    public static partial void Connected(ILogger logger, string host, int port);

    [LoggerMessage(Level = LogLevel.Warning, Message = "RabbitMQ connection attempt {Attempt}/{Max} failed, retrying in {Delay}s")]
    public static partial void ConnectionRetry(ILogger logger, Exception exception, int attempt, int max, double delay);
}
