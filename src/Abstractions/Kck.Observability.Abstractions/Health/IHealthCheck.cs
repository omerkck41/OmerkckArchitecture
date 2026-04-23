namespace Kck.Observability.Abstractions;

public interface IHealthCheck
{
    Task<HealthCheckResult> CheckAsync(CancellationToken ct = default);
}
