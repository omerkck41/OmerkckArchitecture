using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kck.EventBus.RabbitMq.HealthChecks;

/// <summary>
/// Readiness check that reports whether the event bus is ready to consume: its shared RabbitMQ connection is open
/// and, when events are subscribed, its consumer channel is open.
/// </summary>
/// <remarks>
/// It opens no connection of its own and makes no network call. It is Unhealthy until the consumer hosted service
/// has started the consumers, while the connection recovers, and after a channel-level error (for example a
/// consumer_timeout or a 406) closed the consumer channel, which the client does not reopen: restart the host then.
/// </remarks>
internal sealed class RabbitMqHealthCheck(RabbitMqEventBus bus, RabbitMqOptions options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"{options.HostName}:{options.Port}";

        HealthCheckResult result;
        if (bus.IsReady)
            result = HealthCheckResult.Healthy($"RabbitMQ connection to '{endpoint}' is open.");
        else if (!bus.IsConnected)
            result = HealthCheckResult.Unhealthy($"RabbitMQ event bus is not connected to '{endpoint}'.");
        else if (bus.ConsumeChannel is null)
            result = HealthCheckResult.Unhealthy(
                $"RabbitMQ event bus connection to '{endpoint}' is open but the consumers have not started yet.");
        else
            result = HealthCheckResult.Unhealthy(
                $"RabbitMQ event bus connection to '{endpoint}' is open but the consumer channel is closed; restart the host.");

        return Task.FromResult(result);
    }
}
