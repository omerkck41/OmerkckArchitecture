using Microsoft.Extensions.Diagnostics.HealthChecks;
using KckHealthCheckResult = Kck.Observability.Abstractions.HealthCheckResult;
using KckHealthStatus = Kck.Observability.Abstractions.HealthStatus;
using MsHealthCheckResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;

namespace Kck.Observability.OpenTelemetry.Health;

/// <summary>
/// Adapts a Kck <see cref="Kck.Observability.Abstractions.IHealthCheck"/> or delegate to the Microsoft.Extensions health check contract.
/// </summary>
internal sealed class KckHealthCheckAdapter : IHealthCheck
{
    private readonly Func<CancellationToken, Task<KckHealthCheckResult>> _check;

    /// <summary>Initializes the adapter with a raw async delegate that returns a <see cref="KckHealthCheckResult"/>.</summary>
    public KckHealthCheckAdapter(Func<CancellationToken, Task<KckHealthCheckResult>> check)
    {
        _check = check;
    }

    /// <summary>Initializes the adapter by wrapping the <see cref="Kck.Observability.Abstractions.IHealthCheck.CheckAsync"/> method.</summary>
    public KckHealthCheckAdapter(Kck.Observability.Abstractions.IHealthCheck check)
    {
        _check = check.CheckAsync;
    }

    /// <summary>
    /// Executes the underlying Kck health check and maps its result to a <see cref="MsHealthCheckResult"/>.
    /// </summary>
    public async Task<MsHealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await _check(cancellationToken);

        return result.Status switch
        {
            KckHealthStatus.Healthy => MsHealthCheckResult.Healthy(result.Description),
            KckHealthStatus.Degraded => MsHealthCheckResult.Degraded(result.Description),
            _ => MsHealthCheckResult.Unhealthy(result.Description)
        };
    }
}
