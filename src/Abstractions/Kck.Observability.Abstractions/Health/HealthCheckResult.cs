namespace Kck.Observability.Abstractions;

/// <summary>
/// Represents the outcome of a health check, including status, an optional human-readable description, and diagnostic data.
/// </summary>
public sealed record HealthCheckResult(
    HealthStatus Status,
    string? Description = null,
    IReadOnlyDictionary<string, object>? Data = null);

/// <summary>
/// Indicates the health state reported by a health check.
/// </summary>
public enum HealthStatus
{
    /// <summary>The component is operating normally.</summary>
    Healthy,

    /// <summary>The component is operational but experiencing a non-critical issue that may require attention.</summary>
    Degraded,

    /// <summary>The component has failed and is not able to serve requests.</summary>
    Unhealthy
}
