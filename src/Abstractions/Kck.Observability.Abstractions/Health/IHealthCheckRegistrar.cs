namespace Kck.Observability.Abstractions;

public interface IHealthCheckRegistrar
{
    IHealthCheckRegistrar AddCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> check);
    IHealthCheckRegistrar AddCheck(string name, IHealthCheck check);
    IHealthCheckRegistrar AddDependency(string name, string endpoint, HealthCheckType type);
}
