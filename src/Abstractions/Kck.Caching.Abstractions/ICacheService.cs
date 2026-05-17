namespace Kck.Caching.Abstractions;

/// <summary>Storage-agnostic cache abstraction; implementations can target in-memory, Redis, or any other backing store.</summary>
public interface ICacheService
{
    /// <summary>Retrieves the value associated with <paramref name="key"/>, or <see langword="default"/> if absent.</summary>
    ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Stores <paramref name="value"/> under <paramref name="key"/> with an optional TTL override.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);

    /// <summary>Removes the entry identified by <paramref name="key"/>; no-op if the key does not exist.</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>Returns <see langword="true"/> if an entry for <paramref name="key"/> currently exists in the store.</summary>
    ValueTask<bool> ExistsAsync(string key, CancellationToken ct = default);

    /// <summary>Removes all entries whose keys begin with <paramref name="prefix"/>.</summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);

    /// <summary>Stampede-safe read-through: returns the cached value or invokes <paramref name="factory"/> to populate and return it.</summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default);
}
