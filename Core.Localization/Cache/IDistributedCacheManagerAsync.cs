namespace Core.Localization.Cache;

/// <summary>
/// Interface for managing distributed cache operations.
/// </summary>
public interface IDistributedCacheManagerAsync
{
    /// <summary>
    /// Retrieves an item from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <returns>The cached item, or null if not found.</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets an item in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the item to set.</typeparam>
    /// <param name="key">The key of the item to set.</param>
    /// <param name="value">The value of the item to set.</param>
    /// <param name="expiration">The expiration time for the cached item.</param>
    Task SetAsync<T>(string key, T value, TimeSpan expiration);

    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    Task RemoveAsync(string key);
}
