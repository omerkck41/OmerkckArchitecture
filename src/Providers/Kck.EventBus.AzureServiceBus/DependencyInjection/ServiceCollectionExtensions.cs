using Kck.EventBus.Abstractions;
using Kck.EventBus.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering Azure Service Bus event bus services.
/// </summary>
public static class KckEventBusAzureServiceBusServiceCollectionExtensions
{
    /// <summary>
    /// Configures the event bus to use Azure Service Bus as the transport.
    /// </summary>
    public static KckEventBusBuilder UseAzureServiceBus(
        this KckEventBusBuilder builder,
        Action<AzureServiceBusOptions> configure)
    {
        var options = new AzureServiceBusOptions();
        configure(options);

        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString, nameof(options.ConnectionString));

        builder.Services.TryAddSingleton(options);
        builder.Services.TryAddSingleton<AzureServiceBusEventBus>();
        builder.Services.TryAddSingleton<IEventBus>(sp => sp.GetRequiredService<AzureServiceBusEventBus>());
        builder.Services.AddHostedService<AzureServiceBusConsumerHostedService>();

        return builder;
    }
}
