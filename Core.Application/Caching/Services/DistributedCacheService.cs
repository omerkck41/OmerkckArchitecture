using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Core.Application.Caching.Services;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public DistributedCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedData = await _distributedCache.GetStringAsync(key, cancellationToken);
        return cachedData is null ? default : JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var serializedData = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };
        await _distributedCache.SetStringAsync(key, serializedData, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return _distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _distributedCache.GetStringAsync(key, cancellationToken) != null;
    }
}
