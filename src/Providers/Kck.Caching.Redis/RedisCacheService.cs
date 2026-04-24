using System.Text.Json;
using Kck.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Kck.Caching.Redis;

public sealed class RedisCacheService(
    IDistributedCache cache,
    IConnectionMultiplexer redis,
    IOptionsMonitor<CacheOptions> options) : CacheServiceBase
{
    protected override CacheOptions Options { get; } = options.CurrentValue;

    public override async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : default
    {
        var data = await cache.GetStringAsync(BuildKey(key), ct).ConfigureAwait(false);
        return data is null ? default : JsonSerializer.Deserialize<T>(data);
    }

    public override async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var exp = expiration ?? Options.DefaultExpiration;
        var data = JsonSerializer.Serialize(value);
        var entry = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = exp };
        await cache.SetStringAsync(BuildKey(key), data, entry, ct).ConfigureAwait(false);
    }

    public override async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await cache.RemoveAsync(BuildKey(key), ct).ConfigureAwait(false);
    }

    public override async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return await cache.GetStringAsync(BuildKey(key), ct).ConfigureAwait(false) is not null;
    }

    private const int DeleteChunkSize = 1000;

    public override async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var fullPrefix = BuildKey(prefix);
        var db = redis.GetDatabase();
        var endpoints = redis.GetEndPoints();

        foreach (var server in endpoints.Select(e => redis.GetServer(e)))
        {
            var batch = new List<RedisKey>(DeleteChunkSize);

            await foreach (var key in server.KeysAsync(pattern: $"{fullPrefix}*").WithCancellation(ct))
            {
                batch.Add(key);
                if (batch.Count >= DeleteChunkSize)
                {
                    await db.KeyDeleteAsync(batch.ToArray()).ConfigureAwait(false);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
                await db.KeyDeleteAsync(batch.ToArray()).ConfigureAwait(false);
        }
    }
}
