namespace Kck.Observability.Abstractions;

public interface IMetricsService
{
    ICounter CreateCounter(string name, string? description = null, string[]? tags = null);
    IHistogram CreateHistogram(string name, string? description = null, double[]? buckets = null);
    IGauge CreateGauge(string name, string? description = null);
}
