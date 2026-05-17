using System.Diagnostics.Metrics;
using Kck.Observability.Abstractions;

namespace Kck.Observability.OpenTelemetry.Metrics;

/// <summary>
/// OpenTelemetry-backed implementation of <see cref="ICounter"/> that delegates to a <see cref="Counter{T}"/>.
/// </summary>
internal sealed class OpenTelemetryCounter(Counter<double> counter) : ICounter
{
    /// <summary>Increments the counter by <paramref name="value"/> with the given optional tags.</summary>
    public void Increment(double value = 1, params KeyValuePair<string, object?>[] tags)
    {
        counter.Add(value, tags);
    }
}

/// <summary>
/// OpenTelemetry-backed implementation of <see cref="IHistogram"/> that delegates to a <see cref="Histogram{T}"/>.
/// </summary>
internal sealed class OpenTelemetryHistogram(Histogram<double> histogram) : IHistogram
{
    /// <summary>Records a single <paramref name="value"/> observation with the given optional tags.</summary>
    public void Record(double value, params KeyValuePair<string, object?>[] tags)
    {
        histogram.Record(value, tags);
    }
}

/// <summary>
/// OpenTelemetry-backed implementation of <see cref="IGauge"/> that publishes the last set value via an observable gauge instrument.
/// </summary>
internal sealed class OpenTelemetryGauge : IGauge
{
    private double _value;
    private KeyValuePair<string, object?>[] _tags = [];

    /// <summary>Registers an observable gauge on <paramref name="meter"/> that reports the most recently set value.</summary>
    internal OpenTelemetryGauge(Meter meter, string name, string? description)
    {
        meter.CreateObservableGauge(name, () => new Measurement<double>(_value, _tags), description: description);
    }

    /// <summary>Updates the gauge to <paramref name="value"/> and replaces the associated tags.</summary>
    public void Set(double value, params KeyValuePair<string, object?>[] tags)
    {
        _value = value;
        _tags = tags;
    }
}
