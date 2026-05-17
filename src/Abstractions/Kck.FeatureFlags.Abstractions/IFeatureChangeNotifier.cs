namespace Kck.FeatureFlags.Abstractions;

/// <summary>
/// Streams real-time notifications whenever a feature flag's enabled state changes.
/// </summary>
public interface IFeatureChangeNotifier
{
    /// <summary>Returns an async stream that yields a <see cref="FeatureChangeEvent"/> each time any feature flag is toggled.</summary>
    IAsyncEnumerable<FeatureChangeEvent> WatchAsync(CancellationToken ct = default);
}

/// <summary>
/// Represents a single state-change event for a feature flag, capturing the old and new values with a timestamp.
/// </summary>
public sealed record FeatureChangeEvent(string FeatureName, bool OldValue, bool NewValue, DateTimeOffset Timestamp);
