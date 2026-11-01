namespace Kck.EventBus.RabbitMq;

/// <summary>
/// Configuration options for the RabbitMQ event bus provider.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>
    /// RabbitMQ server hostname.
    /// </summary>
    public string HostName { get; set; } = string.Empty;

    /// <summary>
    /// RabbitMQ server port. Default is 5672.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Authentication username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Authentication password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// RabbitMQ virtual host. Default is "/".
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Name of the exchange to publish events to. Default is "kck.eventbus".
    /// </summary>
    public string ExchangeName { get; set; } = "kck.eventbus";

    /// <summary>
    /// Type of the exchange. Default is "topic".
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Prefix for queue names. Default is "kck".
    /// </summary>
    public string QueuePrefix { get; set; } = "kck";

    /// <summary>
    /// Number of connection retry attempts. Default is 5.
    /// </summary>
    public int RetryCount { get; set; } = 5;

    /// <summary>
    /// Delay between connection retry attempts. Default is 2 seconds.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Maximum delay between connection retry attempts (cap for exponential backoff).
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of deliveries of a failing message, including the first one; must be at least 1. After the
    /// last failed delivery the message is rejected without requeue and goes to the dead-letter queue (or is dropped
    /// when <see cref="DeadLetterEnabled"/> is <see langword="false"/> and no broker policy sets a dead-letter
    /// exchange). The deliveries are counted per <c>MessageId</c> in the consuming process and follow each other
    /// immediately (no back-off). Default is 5.
    /// </summary>
    public int MaxDeliveryCount { get; set; } = 5;

    /// <summary>
    /// Maximum unacknowledged messages the consumer channel takes from the broker (basic.qos).
    /// 0 = unlimited (pre-5.0 behaviour). Default is 10.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;

    /// <summary>
    /// When <see langword="true"/> (default) the consumer queues are declared with an
    /// <c>x-dead-letter-exchange</c> argument, and the dead-letter exchange and queue are declared and bound,
    /// so a message rejected after <see cref="MaxDeliveryCount"/> deliveries is kept in the dead-letter queue.
    /// Set it to <see langword="false"/> to declare consumer queues without arguments (pre-5.0 shape), for
    /// example when a broker policy applies the dead-letter exchange.
    /// </summary>
    public bool DeadLetterEnabled { get; set; } = true;

    /// <summary>
    /// Name of the dead-letter exchange (fanout). <see langword="null"/> (default) uses
    /// <c>{QueuePrefix}.dlx</c>, one per service: services that share <see cref="ExchangeName"/> must not share a
    /// fanout dead-letter exchange, or every service's dead-letter queue receives every rejected message.
    /// </summary>
    public string? DeadLetterExchangeName { get; set; }

    /// <summary>
    /// Name of the dead-letter queue bound to the dead-letter exchange. <see langword="null"/> (default) uses
    /// <c>{QueuePrefix}.dlq</c>.
    /// </summary>
    public string? DeadLetterQueueName { get; set; }

    internal string EffectiveDeadLetterExchangeName => DeadLetterExchangeName ?? $"{QueuePrefix}.dlx";

    internal string EffectiveDeadLetterQueueName => DeadLetterQueueName ?? $"{QueuePrefix}.dlq";
}
