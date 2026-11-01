using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Kck.Observability.Abstractions;

namespace Kck.Observability.OpenTelemetry.Metrics;

/// <summary>
/// OpenTelemetry implementation of <see cref="IMetricsService"/> that creates counters, histograms, and gauges backed by a <see cref="Meter"/>.
/// Instruments are cached by name (case-insensitive, as OpenTelemetry instrument names are): a second call with the same
/// name returns the instrument created by the first, whose description and buckets win.
/// </summary>
internal sealed class OpenTelemetryMetricsService : IMetricsService, IDisposable
{
    private readonly Meter _meter;

    // Lazy: GetOrAdd may run its factory twice under contention, and every factory run registers an instrument on the Meter.
    private readonly ConcurrentDictionary<string, Lazy<ICounter>> _counters = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Lazy<IHistogram>> _histograms = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Lazy<IGauge>> _gauges = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes the service with a new <see cref="Meter"/> scoped to <paramref name="serviceName"/>, defaulting to <c>Kck.Application</c>.
    /// </summary>
    public OpenTelemetryMetricsService(string? serviceName = null)
    {
        _meter = new Meter(serviceName ?? "Kck.Application");
    }

    /// <summary>Creates the named <see cref="ICounter"/> on first use and returns the same instance afterwards.</summary>
    public ICounter CreateCounter(string name, string? description = null) =>
        _counters.GetOrAdd(name, key => new Lazy<ICounter>(
            () => new OpenTelemetryCounter(_meter.CreateCounter<double>(key, description: description)))).Value;

    /// <summary>
    /// Creates the named <see cref="IHistogram"/> on first use and returns the same instance afterwards.
    /// <paramref name="buckets"/> are published as the instrument's explicit bucket boundary advice.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="buckets"/> is not strictly increasing or contains a value that is not finite.</exception>
    public IHistogram CreateHistogram(string name, string? description = null, double[]? buckets = null)
    {
        // Before the cache: an invalid call must not leave a failed entry that every later call for the name rethrows.
        ValidateBuckets(buckets);
        return _histograms.GetOrAdd(name, key => new Lazy<IHistogram>(() =>
        {
            var advice = buckets is null ? null : new InstrumentAdvice<double> { HistogramBucketBoundaries = buckets };
            return new OpenTelemetryHistogram(_meter.CreateHistogram(key, unit: null, description, tags: null, advice));
        })).Value;
    }

    /// <summary>Creates the named <see cref="IGauge"/> (an observable gauge on the <see cref="Meter"/>) on first use and returns the same instance afterwards.</summary>
    public IGauge CreateGauge(string name, string? description = null) =>
        _gauges.GetOrAdd(name, key => new Lazy<IGauge>(
            () => new OpenTelemetryGauge(_meter, key, description))).Value;

    public void Dispose() => _meter.Dispose();

    private static void ValidateBuckets(double[]? buckets)
    {
        if (buckets is null)
            return;

        for (var i = 0; i < buckets.Length; i++)
        {
            if (!double.IsFinite(buckets[i]))
                throw new ArgumentException($"Histogram bucket boundaries must be finite numbers; buckets[{i}] is {buckets[i]}.", nameof(buckets));
            if (i > 0 && buckets[i] <= buckets[i - 1])
                throw new ArgumentException(
                    $"Histogram bucket boundaries must be strictly increasing; buckets[{i}] ({buckets[i]}) is not greater than buckets[{i - 1}] ({buckets[i - 1]}).",
                    nameof(buckets));
        }
    }
}
