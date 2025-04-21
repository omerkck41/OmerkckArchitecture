using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Core.Localization.Extensions;

public static class DistributedCacheExtensions
{
    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key)
    {
        var json = await cache.GetStringAsync(key);
        return json == null
            ? default
            : JsonSerializer.Deserialize<T>(json);
    }

    public static Task SetAsync<T>(
        this IDistributedCache cache,
        string key,
        T value,
        TimeSpan absoluteExpirationRelativeToNow)
    {
        var json = JsonSerializer.Serialize(value);
        return cache.SetStringAsync(
            key,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            });
    }
}