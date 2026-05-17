namespace Kck.Observability.Abstractions;

/// <summary>
/// Represents a monotonically increasing counter instrument for tracking event occurrences or cumulative totals.
/// </summary>
public interface ICounter
{
    /// <summary>Increments the counter by <paramref name="value"/> (default 1), optionally annotated with dimension <paramref name="tags"/>.</summary>
    void Increment(double value = 1, params KeyValuePair<string, object?>[] tags);
}

/// <summary>
/// Represents a histogram instrument for recording the statistical distribution of values such as request durations or payload sizes.
/// </summary>
public interface IHistogram
{
    /// <summary>Records a single observation of <paramref name="value"/>, optionally annotated with dimension <paramref name="tags"/>.</summary>
    void Record(double value, params KeyValuePair<string, object?>[] tags);
}

/// <summary>
/// Represents a gauge instrument for recording an arbitrary point-in-time value that can increase or decrease, such as queue depth or memory usage.
/// </summary>
public interface IGauge
{
    /// <summary>Sets the current gauge reading to <paramref name="value"/>, optionally annotated with dimension <paramref name="tags"/>.</summary>
    void Set(double value, params KeyValuePair<string, object?>[] tags);
}
