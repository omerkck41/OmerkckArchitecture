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
    /// Looks <paramref name="key"/> up and reports whether the store has an entry for it, so that a stored default
    /// value (<c>0</c>, <see langword="false"/>, <see cref="Guid.Empty"/>) is not taken for a miss. The default
    /// implementation treats a <see langword="null"/> result of <see cref="GetAsync{T}"/> as "not found" and, when the
    /// value equals <c>default(T)</c>, asks <see cref="ExistsAsync"/> whether the key is stored (a second round trip on
    /// that path only). A provider that can report presence in one call overrides it (InMemory and Redis do).
    /// </summary>
    protected virtual async ValueTask<(bool Found, T? Value)> TryGetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await GetAsync<T>(key, ct).ConfigureAwait(false);
        if (value is null)
            return (false, value);

        if (!EqualityComparer<T>.Default.Equals(value, default!))
            return (true, value);

        // 0, false, Guid.Empty...: only the store can tell a stored default from a missing key.
        return (await ExistsAsync(key, ct).ConfigureAwait(false), value);
    }

    /// <summary>
    /// Returns the cached value for <paramref name="key"/> if present; otherwise invokes <paramref name="factory"/>,
    /// stores the result, and returns it. Uses per-stripe locking to prevent cache stampedes. Presence comes from
    /// <see cref="TryGetAsync{T}"/>, so a cached <c>0</c> or <see langword="false"/> is a hit; a cached
    /// <see langword="null"/> is still treated as a miss and the factory runs again.
    /// </summary>
    public virtual async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var (found, existing) = await TryGetAsync<T>(key, ct).ConfigureAwait(false);
        if (found && existing is not null)
            return existing;

        var semaphore = GetStripe(BuildKey(key));
        await semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            (found, existing) = await TryGetAsync<T>(key, ct).ConfigureAwait(false);
            if (found && existing is not null)
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
