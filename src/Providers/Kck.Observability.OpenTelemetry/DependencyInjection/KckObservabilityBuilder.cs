namespace Microsoft.Extensions.DependencyInjection;

public sealed class KckObservabilityBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    public KckObservabilityBuilder UseOpenTelemetry(Action<KckOpenTelemetryBuilder> configure)
    {
        var builder = new KckOpenTelemetryBuilder(Services);
        configure(builder);
        builder.Build();
        return this;
    }

    public KckObservabilityBuilder UseHealthChecks(Action<KckHealthChecksBuilder> configure)
    {
        var builder = new KckHealthChecksBuilder(Services);
        configure(builder);
        return this;
    }
}
