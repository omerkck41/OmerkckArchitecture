using System.Diagnostics.Metrics;
using Kck.Observability.Abstractions;

namespace Kck.Observability.OpenTelemetry.Metrics;

/// <summary>
/// OpenTelemetry implementation of <see cref="IMetricsService"/> that creates counters, histograms, and gauges backed by a <see cref="Meter"/>.
/// </summary>
internal sealed class OpenTelemetryMetricsService : IMetricsService, IDisposable
{
    private readonly Meter _meter;

    /// <summary>
    /// Initializes the service with a new <see cref="Meter"/> scoped to <paramref name="serviceName"/>, defaulting to <c>Kck.Application</c>.
    /// </summary>
    public OpenTelemetryMetricsService(string? serviceName = null)
    {
        _meter = new Meter(serviceName ?? "Kck.Application");
    }

    /// <summary>Creates and returns a new <see cref="ICounter"/> instrument with the specified name and optional description.</summary>
    public ICounter CreateCounter(string name, string? description = null, string[]? tags = null)
    {
        var counter = _meter.CreateCounter<double>(name, description: description);
        return new OpenTelemetryCounter(counter);
    }

    /// <summary>Creates and returns a new <see cref="IHistogram"/> instrument with the specified name and optional description.</summary>
    public IHistogram CreateHistogram(string name, string? description = null, double[]? buckets = null)
    {
        var histogram = _meter.CreateHistogram<double>(name, description: description);
        return new OpenTelemetryHistogram(histogram);
    }

    /// <summary>Creates and returns a new <see cref="IGauge"/> instrument backed by an observable gauge on the underlying <see cref="Meter"/>.</summary>
    public IGauge CreateGauge(string name, string? description = null)
    {
        return new OpenTelemetryGauge(_meter, name, description);
    }

    public void Dispose() => _meter.Dispose();
}
