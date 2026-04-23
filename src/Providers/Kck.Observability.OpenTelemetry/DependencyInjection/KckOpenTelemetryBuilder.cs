using Kck.Observability.Abstractions;
using Kck.Observability.OpenTelemetry.Metrics;
using Kck.Observability.OpenTelemetry.Tracing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public sealed class KckOpenTelemetryBuilder(IServiceCollection services)
{
    public string ServiceName { get; set; } = "KckApp";
    private bool _enableTracing;
    private bool _enableMetrics;
    private string? _otlpEndpoint;

    public KckOpenTelemetryBuilder EnableTracing()
    {
        _enableTracing = true;
        return this;
    }

    public KckOpenTelemetryBuilder EnableMetrics()
    {
        _enableMetrics = true;
        return this;
    }

    public KckOpenTelemetryBuilder UseOtlpExporter(string endpoint)
    {
        _otlpEndpoint = endpoint;
        return this;
    }

    internal void Build()
    {
        var otel = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName));

        if (_enableTracing)
        {
            otel.WithTracing(tracing =>
            {
                tracing
                    .AddSource(ServiceName)
                    .AddSource("Kck.Application")
                    .AddSource("Microsoft.EntityFrameworkCore")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true);

                if (_otlpEndpoint is not null)
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(_otlpEndpoint));
            });
        }

        if (_enableMetrics)
        {
            otel.WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(ServiceName)
                    .AddMeter("Kck.Application")
                    .AddAspNetCoreInstrumentation();

                if (_otlpEndpoint is not null)
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(_otlpEndpoint));
            });
        }

        services.TryAddSingleton<ITracingService>(new OpenTelemetryTracingService(ServiceName));
        services.TryAddSingleton<IMetricsService>(new OpenTelemetryMetricsService(ServiceName));
    }
}
