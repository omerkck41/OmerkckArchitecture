namespace Kck.Caching.Abstractions;

/// <summary>
/// Base implementation of <see cref="ICacheService"/> that provides stampede-safe <see cref="GetOrSetAsync{T}"/>
/// via a 64-stripe <see cref="System.Threading.SemaphoreSlim"/> array. Subclasses supply the storage-specific operations.
/// </summary>
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

    /// <summary>Cache configuration provided by the concrete subclass.</summary>
    protected abstract CacheOptions Options { get; }

    /// <inheritdoc/>
    public abstract ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <inheritdoc/>
    public abstract Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);

    /// <inheritdoc/>
    public abstract Task RemoveAsync(string key, CancellationToken ct = default);

    /// <inheritdoc/>
    public abstract ValueTask<bool> ExistsAsync(string key, CancellationToken ct = default);

    /// <inheritdoc/>
    public abstract Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);

    /// <summary>
    /// Returns the cached value for <paramref name="key"/> if present; otherwise invokes <paramref name="factory"/>,
    /// stores the result, and returns it. Uses per-stripe locking to prevent cache stampedes.
    /// </summary>
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

    /// <summary>Prepends <see cref="CacheOptions.KeyPrefix"/> to <paramref name="key"/> when a prefix is configured.</summary>
    protected string BuildKey(string key) =>
        string.IsNullOrEmpty(Options.KeyPrefix) ? key : $"{Options.KeyPrefix}{key}";
}
