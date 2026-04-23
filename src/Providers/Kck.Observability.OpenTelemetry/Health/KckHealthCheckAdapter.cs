using Microsoft.Extensions.Diagnostics.HealthChecks;
using KckHealthCheckResult = Kck.Observability.Abstractions.HealthCheckResult;
using KckHealthStatus = Kck.Observability.Abstractions.HealthStatus;
using MsHealthCheckResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;

namespace Kck.Observability.OpenTelemetry.Health;

internal sealed class KckHealthCheckAdapter : IHealthCheck
{
    private readonly Func<CancellationToken, Task<KckHealthCheckResult>> _check;

    public KckHealthCheckAdapter(Func<CancellationToken, Task<KckHealthCheckResult>> check)
    {
        _check = check;
    }

    public KckHealthCheckAdapter(Kck.Observability.Abstractions.IHealthCheck check)
    {
        _check = check.CheckAsync;
    }

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
