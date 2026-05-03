namespace Kck.Caching.Abstractions;

public abstract class CacheServiceBase : ICacheService
{
    // 64-stripe lock array: constant memory, zero external deps, correct for cache stampede prevention.
    // Instance-level (not static) so InMemory and Redis providers don't share stripes and
    // unnecessarily serialize each other's GetOrSetAsync calls.
    // False serialization (two distinct keys → same stripe) only reduces parallelism, never correctness.
    // Power-of-two count enables cheap bitwise modulo.
    private readonly SemaphoreSlim[] _stripes =
        Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    private SemaphoreSlim GetStripe(string key) =>
        _stripes[(uint)key.GetHashCode() % (uint)_stripes.Length];

    protected abstract CacheOptions Options { get; }

    public abstract ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default);
    public abstract Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    public abstract Task RemoveAsync(string key, CancellationToken ct = default);
    public abstract ValueTask<bool> ExistsAsync(string key, CancellationToken ct = default);
    public abstract Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var existing = await GetAsync<T>(key, ct).ConfigureAwait(false);
        if (existing is not null)
            return existing;

        var semaphore = GetStripe(BuildKey(key));
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
        }
    }

    protected string BuildKey(string key) =>
        string.IsNullOrEmpty(Options.KeyPrefix) ? key : $"{Options.KeyPrefix}{key}";
}
