using System.Diagnostics.Metrics;
using Kck.Observability.Abstractions;

namespace Kck.Observability.OpenTelemetry.Metrics;

internal sealed class OpenTelemetryMetricsService : IMetricsService
{
    private readonly Meter _meter;

    public OpenTelemetryMetricsService(string? serviceName = null)
    {
        _meter = new Meter(serviceName ?? "Kck.Application");
    }

    public ICounter CreateCounter(string name, string? description = null, string[]? tags = null)
    {
        var counter = _meter.CreateCounter<double>(name, description: description);
        return new OpenTelemetryCounter(counter);
    }

    public IHistogram CreateHistogram(string name, string? description = null, double[]? buckets = null)
    {
        var histogram = _meter.CreateHistogram<double>(name, description: description);
        return new OpenTelemetryHistogram(histogram);
    }

    public IGauge CreateGauge(string name, string? description = null)
    {
        return new OpenTelemetryGauge(_meter, name, description);
    }
}
