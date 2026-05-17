namespace Kck.Observability.Abstractions;

/// <summary>
/// Fluent builder for registering health checks with the health monitoring system.
/// </summary>
public interface IHealthCheckRegistrar
{
    /// <summary>Registers an inline delegate-based health check under the given <paramref name="name"/>.</summary>
    IHealthCheckRegistrar AddCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> check);

    /// <summary>Registers an <see cref="IHealthCheck"/> instance under the given <paramref name="name"/>.</summary>
    IHealthCheckRegistrar AddCheck(string name, IHealthCheck check);

    /// <summary>Registers a dependency probe for the specified <paramref name="endpoint"/> with its associated <paramref name="type"/>.</summary>
    IHealthCheckRegistrar AddDependency(string name, string endpoint, HealthCheckType type);
}
