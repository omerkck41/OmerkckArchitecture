using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kck.Persistence.EntityFramework.HealthChecks;

/// <summary>
/// Health check that verifies the database connection for a specific <see cref="DbContext"/>.
/// </summary>
internal sealed class EfCoreHealthCheck<TContext>(TContext context) : IHealthCheck
    where TContext : DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await context.Database
                .CanConnectAsync(cancellationToken)
                .ConfigureAwait(false);

            return canConnect
                ? HealthCheckResult.Healthy($"Database '{typeof(TContext).Name}' is reachable.")
                : HealthCheckResult.Unhealthy($"Database '{typeof(TContext).Name}' is unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database '{typeof(TContext).Name}' check failed.", ex);
        }
    }
}
