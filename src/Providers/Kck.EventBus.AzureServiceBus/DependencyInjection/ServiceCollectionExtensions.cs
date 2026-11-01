using Azure.Messaging.ServiceBus;
using Kck.EventBus.Abstractions;
using Kck.EventBus.AzureServiceBus;
using Kck.EventBus.AzureServiceBus.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering Azure Service Bus event bus services.
/// </summary>
public static class KckEventBusAzureServiceBusServiceCollectionExtensions
{
    /// <summary>
    /// Configures the event bus to use Azure Service Bus as the transport. When the container has a
    /// <see cref="ServiceBusClient"/> registered, the bus uses that client (register your own client for a token
    /// credential such as managed identity, or for custom client options) and <c>ConnectionString</c> may stay empty;
    /// register the client before calling this method. Otherwise the bus builds its own client from
    /// <c>ConnectionString</c>.
    /// </summary>
    public static KckEventBusBuilder UseAzureServiceBus(
        this KckEventBusBuilder builder,
        Action<AzureServiceBusOptions> configure)
    {
        var options = new AzureServiceBusOptions();
        configure(options);

        // Eager check: throw ArgumentException immediately at registration time so the
        // existing test contract (and developer experience) is preserved. A ServiceBusClient registered before this
        // call replaces the connection string; a keyed registration does not (the bus resolves the unkeyed client).
        if (!builder.Services.Any(d => d.ServiceType == typeof(ServiceBusClient) && !d.IsKeyedService))
            ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString, nameof(options.ConnectionString)); // NOSONAR S3236 - explicit name gives "ConnectionString"; CallerArgumentExpression would give "options.ConnectionString"

        // IValidateOptions provides defence-in-depth and richer messages at app startup.
        builder.Services.Configure<AzureServiceBusOptions>(configure);
        builder.Services.TryAddSingleton<IValidateOptions<AzureServiceBusOptions>, AzureServiceBusOptionsValidator>();
        builder.Services.AddOptions<AzureServiceBusOptions>().ValidateOnStart();

        builder.Services.TryAddSingleton(options);
        builder.Services.TryAddSingleton(sp =>
        {
            var busOptions = sp.GetRequiredService<AzureServiceBusOptions>();
            var logger = sp.GetRequiredService<ILogger<AzureServiceBusEventBus>>();
            return sp.GetService<ServiceBusClient>() is { } client
                ? new AzureServiceBusEventBus(busOptions, sp, logger, client)
                : new AzureServiceBusEventBus(busOptions, sp, logger);
        });
        builder.Services.TryAddSingleton<IEventBus>(sp => sp.GetRequiredService<AzureServiceBusEventBus>());
        builder.Services.AddHostedService<AzureServiceBusConsumerHostedService>();

        return builder;
    }
}
