using Microsoft.Extensions.Logging;

namespace Kck.EventBus.RabbitMq;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Published {EventName} with Id {EventId}")]
    public static partial void Published(ILogger logger, string eventName, Guid eventId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error processing {EventName}")]
    public static partial void ProcessingError(ILogger logger, Exception exception, string eventName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Message {MessageId} dead-lettered after {Attempts} deliveries (MaxDeliveryCount) to exchange {DeadLetterExchange}")]
    public static partial void MessageDeadLettered(ILogger logger, string? messageId, int attempts, string deadLetterExchange);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Message {MessageId} rejected after {Attempts} deliveries (MaxDeliveryCount); Kck declared no dead-letter exchange for this queue, the broker drops the message unless a policy sets one")]
    public static partial void MessageRejectedWithoutDeadLetter(ILogger logger, string? messageId, int attempts);

    [LoggerMessage(Level = LogLevel.Information, Message = "Subscribed to {EventName} on queue {QueueName}")]
    public static partial void Subscribed(ILogger logger, string eventName, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Connected to RabbitMQ at {Host}:{Port}")]
    public static partial void Connected(ILogger logger, string host, int port);

    [LoggerMessage(Level = LogLevel.Warning, Message = "RabbitMQ connection attempt {Attempt}/{Max} failed, retrying in {Delay}s")]
    public static partial void ConnectionRetry(ILogger logger, Exception exception, int attempt, int max, double delay);

    [LoggerMessage(Level = LogLevel.Information, Message = "Publish channel was closed ({Reason}); opened a new channel")]
    public static partial void PublishChannelReopened(ILogger logger, string reason);

    [LoggerMessage(Level = LogLevel.Warning, Message = "RabbitMQ connection is down and recovering; waiting {Delay}s for it (attempt {Attempt}/{Max})")]
    public static partial void WaitingForConnectionRecovery(ILogger logger, int attempt, int max, double delay);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Consume channel shut down ({Reason}); consumers are stopped and the health check reports Unhealthy until the host restarts (automatic recovery only covers connection loss)")]
    public static partial void ConsumeChannelShutdown(ILogger logger, string reason);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Dead-letter exchange {Exchange} and queue {Queue} declared")]
    public static partial void DeadLetterTopologyDeclared(ILogger logger, string exchange, string queue);
}
