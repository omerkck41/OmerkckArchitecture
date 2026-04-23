using Microsoft.Extensions.Hosting;

namespace Kck.EventBus.RabbitMq;

/// <summary>
/// Background service that starts RabbitMQ event consuming on application startup.
/// </summary>
internal sealed class RabbitMqConsumerHostedService(RabbitMqEventBus eventBus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        => await eventBus.StartConsumingAsync(stoppingToken).ConfigureAwait(false);
}
