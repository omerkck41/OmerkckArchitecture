using StackExchange.Redis;
using System.Text.Json;

namespace Core.Localization.Cache;

/// <summary>
/// Manages Redis cache operations.
/// </summary>
public class RedisCacheManagerAsync : IDistributedCacheManagerAsync
{
    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheManagerAsync"/> class.
    /// </summary>
    /// <param name="redisConnectionString">The Redis connection string.</param>
    public RedisCacheManagerAsync(string redisConnectionString)
    {
        var connection = ConnectionMultiplexer.Connect(redisConnectionString);
        _database = connection.GetDatabase();
    }

    /// <summary>  
    /// Gets the cached value for the specified key.  
    /// </summary>  
    /// <typeparam name="T">The type of the cached value.</typeparam>  
    /// <param name="key">The cache key.</param>  
    /// <returns>The cached value, or default if not found.</returns>  
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return default;

        // Explicitly convert RedisValue to string to resolve ambiguity  
        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    /// <summary>
    /// Sets the cached value for the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">The expiration time for the cached value.</param>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, expiration);
    }

    /// <summary>
    /// Removes the cached value for the specified key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}
