namespace Kck.FeatureFlags.Abstractions;

public interface IFeatureChangeNotifier
{
    IAsyncEnumerable<FeatureChangeEvent> WatchAsync(CancellationToken ct = default);
}

public sealed record FeatureChangeEvent(string FeatureName, bool OldValue, bool NewValue, DateTimeOffset Timestamp);
