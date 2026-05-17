namespace Kck.Observability.Abstractions;

/// <summary>
/// Factory service for creating named metric instruments (counters, histograms, and gauges) backed by the configured metrics provider.
/// </summary>
public interface IMetricsService
{
    /// <summary>Creates or retrieves a named <see cref="ICounter"/> instrument with optional description and default tag keys.</summary>
    ICounter CreateCounter(string name, string? description = null, string[]? tags = null);

    /// <summary>Creates or retrieves a named <see cref="IHistogram"/> instrument with optional description and explicit bucket boundaries.</summary>
    IHistogram CreateHistogram(string name, string? description = null, double[]? buckets = null);

    /// <summary>Creates or retrieves a named <see cref="IGauge"/> instrument with an optional description.</summary>
    IGauge CreateGauge(string name, string? description = null);
}
