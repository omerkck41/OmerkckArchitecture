using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Kck.Caching.Redis.HealthChecks;

/// <summary>
/// Health check that verifies the Redis connection is alive by sending a PING command.
/// </summary>
internal sealed class RedisHealthCheck(IConnectionMultiplexer multiplexer) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await multiplexer.GetDatabase()
                .PingAsync()
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            return HealthCheckResult.Healthy($"Redis PING latency: {latency.TotalMilliseconds:F1} ms");
        }
        catch (RedisException ex)
        {
            return HealthCheckResult.Unhealthy("Redis is unavailable.", ex);
        }
    }
}
