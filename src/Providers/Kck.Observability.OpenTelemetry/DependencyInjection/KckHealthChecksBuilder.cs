using Kck.Observability.OpenTelemetry.Health;

namespace Microsoft.Extensions.DependencyInjection;

public sealed class KckHealthChecksBuilder
{
    internal const string LiveTag = "live";
    internal const string ReadyTag = "ready";
    internal const string StartupTag = "startup";

    private readonly IHealthChecksBuilder _healthChecksBuilder;

    internal KckHealthChecksBuilder(IServiceCollection services)
    {
        _healthChecksBuilder = services.AddHealthChecks();
    }

    public KckHealthChecksBuilder AddCheck(string name, Func<CancellationToken, Task<Kck.Observability.Abstractions.HealthCheckResult>> check)
    {
        _healthChecksBuilder.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
            name,
            _ => new KckHealthCheckAdapter(check),
            failureStatus: null,
            tags: null));
        return this;
    }

    public KckHealthChecksBuilder AddCheck(string name, Kck.Observability.Abstractions.IHealthCheck check)
    {
        _healthChecksBuilder.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
            name,
            _ => new KckHealthCheckAdapter(check),
            failureStatus: null,
            tags: null));
        return this;
    }

    public KckHealthChecksBuilder AddLivenessCheck(string name, Func<CancellationToken, Task<Kck.Observability.Abstractions.HealthCheckResult>> check)
    {
        _healthChecksBuilder.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
            name,
            _ => new KckHealthCheckAdapter(check),
            failureStatus: null,
            tags: [LiveTag]));
        return this;
    }

    public KckHealthChecksBuilder AddReadinessCheck(string name, Func<CancellationToken, Task<Kck.Observability.Abstractions.HealthCheckResult>> check)
    {
        _healthChecksBuilder.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
            name,
            _ => new KckHealthCheckAdapter(check),
            failureStatus: null,
            tags: [ReadyTag]));
        return this;
    }

    public KckHealthChecksBuilder AddStartupCheck(string name, Func<CancellationToken, Task<Kck.Observability.Abstractions.HealthCheckResult>> check)
    {
        _healthChecksBuilder.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
            name,
            _ => new KckHealthCheckAdapter(check),
            failureStatus: null,
            tags: [StartupTag]));
        return this;
    }
}
