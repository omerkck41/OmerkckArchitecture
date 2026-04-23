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
    /// Maximum number of delivery attempts before a message is dead-lettered.
    /// Default is 5.
    /// </summary>
    public int MaxDeliveryCount { get; set; } = 5;
}
