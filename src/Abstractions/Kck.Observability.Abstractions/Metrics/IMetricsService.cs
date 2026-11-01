namespace Kck.Observability.Abstractions;

/// <summary>
/// Factory service for creating named metric instruments (counters, histograms, and gauges) backed by the configured metrics provider.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Creates or retrieves a named <see cref="ICounter"/> instrument with an optional description. A second call with the
    /// same name returns the first instance (its description wins). Tags are passed on each <see cref="ICounter.Increment"/> call.
    /// </summary>
    ICounter CreateCounter(string name, string? description = null);

    /// <summary>
    /// Creates or retrieves a named <see cref="IHistogram"/> instrument with an optional description and explicit bucket
    /// boundaries. A second call with the same name returns the first instance (its description and buckets win).
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="buckets"/> is not strictly increasing (unsorted or duplicate values) or contains NaN or an infinity.
    /// </exception>
    IHistogram CreateHistogram(string name, string? description = null, double[]? buckets = null);

    /// <summary>
    /// Creates or retrieves a named <see cref="IGauge"/> instrument with an optional description. A second call with the
    /// same name returns the first instance (its description wins).
    /// </summary>
    IGauge CreateGauge(string name, string? description = null);
}
