using Kck.EventBus.Abstractions;
using Kck.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering RabbitMQ event bus services.
/// </summary>
public static class KckEventBusRabbitMqServiceCollectionExtensions
{
    /// <summary>
    /// Configures the event bus to use RabbitMQ as the transport.
    /// </summary>
    public static KckEventBusBuilder UseRabbitMq(
        this KckEventBusBuilder builder,
        Action<RabbitMqOptions> configure)
    {
        var options = new RabbitMqOptions();
        configure(options);

        ArgumentException.ThrowIfNullOrWhiteSpace(options.HostName, nameof(options.HostName));

        builder.Services.TryAddSingleton(options);
        builder.Services.TryAddSingleton<RabbitMqEventBus>();
        builder.Services.TryAddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMqEventBus>());
        builder.Services.AddHostedService<RabbitMqConsumerHostedService>();

        return builder;
    }
}
