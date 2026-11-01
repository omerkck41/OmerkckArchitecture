using System.Collections.Concurrent;

namespace Kck.EventBus.RabbitMq;

/// <summary>
/// Counts delivery attempts per message id in the consuming process.
/// </summary>
/// <remarks>
/// <c>basic.nack</c> with requeue does not add an <c>x-death</c> header and classic queues do not count
/// redeliveries, so the consumer has to count them itself. The count lives in memory: it restarts when the process
/// restarts or when another consumer instance receives the redelivery. When more than
/// <c>maxTrackedMessages</c> ids are tracked, a prune (at most once per <c>pruneInterval</c>) removes the entries not
/// seen for longer than <c>maxAge</c> and then, if still over the cap, the least recently seen ones.
/// </remarks>
internal sealed class DeliveryAttemptTracker
{
    private static readonly TimeSpan DefaultMaxAge = TimeSpan.FromHours(1);
    private static readonly TimeSpan DefaultPruneInterval = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);
    private readonly Lock _pruneLock = new();
    private readonly TimeProvider _timeProvider;
    private readonly int _maxTrackedMessages;
    private readonly TimeSpan _maxAge;
    private readonly TimeSpan _pruneInterval;
    private long? _lastPruneTimestamp;

    public DeliveryAttemptTracker(
        TimeProvider timeProvider,
        int maxTrackedMessages = 10_000,
        TimeSpan? maxAge = null,
        TimeSpan? pruneInterval = null)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTrackedMessages);

        _timeProvider = timeProvider;
        _maxTrackedMessages = maxTrackedMessages;
        _maxAge = maxAge ?? DefaultMaxAge;
        _pruneInterval = pruneInterval ?? DefaultPruneInterval;
    }

    /// <summary>Number of tracked message ids.</summary>
    internal int Count => _entries.Count;

    /// <summary>
    /// Records one delivery of <paramref name="key"/> and returns its attempt number (the first call returns 1).
    /// </summary>
    public int Record(string key)
    {
        var now = _timeProvider.GetTimestamp();
        var entry = _entries.AddOrUpdate(
            key,
            static (_, timestamp) => new Entry(1, timestamp),
            static (_, existing, timestamp) => new Entry(existing.Attempts + 1, timestamp),
            now);

        if (_entries.Count > _maxTrackedMessages)
            TryPrune();

        return entry.Attempts;
    }

    /// <summary>Stops tracking <paramref name="key"/> (the message was acknowledged or finally rejected).</summary>
    public void Forget(string key) => _entries.TryRemove(key, out _);

    // One thread prunes; the others go on without waiting. A prune scans the whole dictionary, so it runs at most once
    // per prune interval even when the dictionary stays over the cap.
    private void TryPrune()
    {
        if (!_pruneLock.TryEnter())
            return;

        try
        {
            if (_lastPruneTimestamp is { } last && _timeProvider.GetElapsedTime(last) < _pruneInterval)
                return;

            // TryRemove(pair) removes an entry only if it was not updated meanwhile; ConcurrentDictionary tolerates
            // removal during enumeration.
            foreach (var stale in _entries.Where(pair => _timeProvider.GetElapsedTime(pair.Value.LastSeenTimestamp) > _maxAge))
                _entries.TryRemove(stale);

            var excess = _entries.Count - _maxTrackedMessages;
            if (excess > 0)
            {
                foreach (var pair in _entries.OrderBy(static p => p.Value.LastSeenTimestamp).Take(excess))
                    _entries.TryRemove(pair);
            }

            _lastPruneTimestamp = _timeProvider.GetTimestamp();
        }
        finally
        {
            _pruneLock.Exit();
        }
    }

    private readonly record struct Entry(int Attempts, long LastSeenTimestamp);
}
