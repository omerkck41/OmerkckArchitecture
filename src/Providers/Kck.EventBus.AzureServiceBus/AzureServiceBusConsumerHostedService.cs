using Microsoft.Extensions.Hosting;

namespace Kck.EventBus.AzureServiceBus;

/// <summary>
/// Background service that starts Azure Service Bus event processing on application startup.
/// </summary>
internal sealed class AzureServiceBusConsumerHostedService(AzureServiceBusEventBus eventBus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        => await eventBus.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
}
