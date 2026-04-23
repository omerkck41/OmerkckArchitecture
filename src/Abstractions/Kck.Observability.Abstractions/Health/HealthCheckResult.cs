namespace Kck.Observability.Abstractions;

public sealed record HealthCheckResult(
    HealthStatus Status,
    string? Description = null,
    IReadOnlyDictionary<string, object>? Data = null);

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
