using System.Collections.Concurrent;

namespace Kck.Caching.Abstractions;

public abstract class CacheServiceBase : ICacheService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    protected abstract CacheOptions Options { get; }

    public abstract Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    public abstract Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    public abstract Task RemoveAsync(string key, CancellationToken ct = default);
    public abstract Task<bool> ExistsAsync(string key, CancellationToken ct = default);
    public abstract Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var existing = await GetAsync<T>(key, ct).ConfigureAwait(false);
        if (existing is not null)
            return existing;

        var fullKey = BuildKey(key);
        var semaphore = Locks.GetOrAdd(fullKey, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            existing = await GetAsync<T>(key, ct).ConfigureAwait(false);
            if (existing is not null)
                return existing;

            var value = await factory().ConfigureAwait(false);
            await SetAsync(key, value, expiration, ct).ConfigureAwait(false);
            return value;
        }
        finally
        {
            semaphore.Release();
            // Eviction removed: CurrentCount-based eviction races with concurrent waiters and
            // can cause the same key to map to two different semaphores, breaking mutual exclusion.
            // Trade-off: Locks dictionary grows unbounded for unique keys. For typical cache usage
            // (bounded key space), memory cost is negligible.
        }
    }

    protected string BuildKey(string key) =>
        string.IsNullOrEmpty(Options.KeyPrefix) ? key : $"{Options.KeyPrefix}{key}";
}
