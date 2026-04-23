using System.Diagnostics.Metrics;
using Kck.Observability.Abstractions;

namespace Kck.Observability.OpenTelemetry.Metrics;

internal sealed class OpenTelemetryCounter(Counter<double> counter) : ICounter
{
    public void Increment(double value = 1, params KeyValuePair<string, object?>[] tags)
    {
        counter.Add(value, tags);
    }
}

internal sealed class OpenTelemetryHistogram(Histogram<double> histogram) : IHistogram
{
    public void Record(double value, params KeyValuePair<string, object?>[] tags)
    {
        histogram.Record(value, tags);
    }
}

internal sealed class OpenTelemetryGauge : IGauge
{
    private double _value;
    private KeyValuePair<string, object?>[] _tags = [];

    internal OpenTelemetryGauge(Meter meter, string name, string? description)
    {
        meter.CreateObservableGauge(name, () => new Measurement<double>(_value, _tags), description: description);
    }

    public void Set(double value, params KeyValuePair<string, object?>[] tags)
    {
        _value = value;
        _tags = tags;
    }
}
