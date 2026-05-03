using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Kck.EventBus.RabbitMq.HealthChecks;

/// <summary>
/// Health check that verifies RabbitMQ broker connectivity by opening a transient connection.
/// </summary>
internal sealed class RabbitMqHealthCheck(RabbitMqOptions options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost
            };

            await using var connection = await factory
                .CreateConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            return connection.IsOpen
                ? HealthCheckResult.Healthy($"RabbitMQ connection to '{options.HostName}:{options.Port}' is open.")
                : HealthCheckResult.Unhealthy($"RabbitMQ connection to '{options.HostName}:{options.Port}' is closed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"RabbitMQ broker '{options.HostName}:{options.Port}' is unavailable.", ex);
        }
    }
}
