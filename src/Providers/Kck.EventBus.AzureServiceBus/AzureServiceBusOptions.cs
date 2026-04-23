namespace Kck.EventBus.AzureServiceBus;

/// <summary>
/// Configuration options for the Azure Service Bus event bus provider.
/// </summary>
public sealed class AzureServiceBusOptions
{
    /// <summary>
    /// Azure Service Bus connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Topic name for publishing events. Default is "kck-eventbus".
    /// </summary>
    public string TopicName { get; set; } = "kck-eventbus";

    /// <summary>
    /// Subscription name for consuming events. Default is "default".
    /// </summary>
    public string SubscriptionName { get; set; } = "default";

    /// <summary>
    /// Max concurrent calls for the processor. Default is 10.
    /// </summary>
    public int MaxConcurrentCalls { get; set; } = 10;

    /// <summary>
    /// Max auto lock renewal duration. Default is 5 minutes.
    /// </summary>
    public TimeSpan MaxAutoLockRenewalDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Number of retry attempts. Default is 3.
    /// </summary>
    public int RetryCount { get; set; } = 3;
}
