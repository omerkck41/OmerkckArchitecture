namespace Kck.Observability.Abstractions;

/// <summary>
/// Defines a single health probe that can be registered with the health system and executed on demand or on a schedule.
/// </summary>
public interface IHealthCheck
{
    /// <summary>Executes the health probe and returns a <see cref="HealthCheckResult"/> describing the current state.</summary>
    Task<HealthCheckResult> CheckAsync(CancellationToken ct = default);
}
