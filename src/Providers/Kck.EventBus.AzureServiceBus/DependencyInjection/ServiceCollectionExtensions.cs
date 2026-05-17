using Kck.EventBus.Abstractions;
using Kck.EventBus.AzureServiceBus;
using Kck.EventBus.AzureServiceBus.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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

        builder.Services.Configure<AzureServiceBusOptions>(configure);
        builder.Services.TryAddSingleton<IValidateOptions<AzureServiceBusOptions>, AzureServiceBusOptionsValidator>();
        builder.Services.AddOptions<AzureServiceBusOptions>().ValidateOnStart();

        builder.Services.TryAddSingleton(options);
        builder.Services.TryAddSingleton<AzureServiceBusEventBus>();
        builder.Services.TryAddSingleton<IEventBus>(sp => sp.GetRequiredService<AzureServiceBusEventBus>());
        builder.Services.AddHostedService<AzureServiceBusConsumerHostedService>();

        return builder;
    }
}
