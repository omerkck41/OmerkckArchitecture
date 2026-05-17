using System.Diagnostics;
using System.Text.Json;
using Kck.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Kck.Caching.Hybrid;

/// <summary>
/// ICacheService implementation backed by Microsoft HybridCache (L1 in-process +
/// L2 Redis). GetOrSetAsync uses HybridCache's built-in stampede protection;
/// other methods fall back to the distributed cache or Redis directly.
/// </summary>
[DebuggerDisplay("Prefix={_opts.KeyPrefix,nq}, Provider=HybridCache(L1+L2)")]
public sealed class HybridCacheService(
    HybridCache hybridCache,
    IDistributedCache distributedCache,
    IConnectionMultiplexer redis,
    IOptionsMonitor<HybridCacheOptions> options) : CacheServiceBase
{
    private readonly HybridCacheOptions _opts = options.CurrentValue;

    protected override CacheOptions Options { get; } = new CacheOptions
    {
        KeyPrefix = options.CurrentValue.KeyPrefix,
        DefaultExpiration = options.CurrentValue.DefaultExpiration
    };

    /// <summary>
    /// Retrieves an entry from the distributed cache (L2). L1 is not queried for
    /// plain reads; use <c>GetOrSetAsync</c> to benefit from L1 caching.
    /// </summary>
    public override async ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default)
        where T : default
    {
        var data = await distributedCache.GetStringAsync(BuildKey(key), ct).ConfigureAwait(false);
        return data is null ? default : JsonSerializer.Deserialize<T>(data);
    }

    /// <summary>
    /// Writes to both L1 (in-process) and L2 (Redis) via HybridCache.
    /// </summary>
    public override async Task SetAsync<T>(
        string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var entry = new HybridCacheEntryOptions
        {
            Expiration = expiration ?? _opts.DefaultExpiration,
            LocalCacheExpiration = _opts.DefaultLocalExpiration
        };
        await hybridCache.SetAsync(BuildKey(key), value, entry, cancellationToken: ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Removes an entry from both L1 and L2 via HybridCache.
    /// </summary>
    public override async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await hybridCache.RemoveAsync(BuildKey(key), ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks entry existence using a Redis EXISTS command (O(1), no payload transfer).
    /// </summary>
    public override async ValueTask<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await redis.GetDatabase().KeyExistsAsync(BuildKey(key)).ConfigureAwait(false);
    }

    private const int DeleteChunkSize = 1000;

    /// <summary>
    /// Removes all keys matching the prefix from L2 (Redis SCAN) and evicts
    /// each from L1 via HybridCache.RemoveAsync.
    /// </summary>
    public override async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var fullPrefix = BuildKey(prefix);
        var db = redis.GetDatabase();

        foreach (var server in redis.GetEndPoints().Select(e => redis.GetServer(e)))
        {
            var batch = new List<RedisKey>(DeleteChunkSize);
            var keysToEvict = new List<string>(DeleteChunkSize);

            await foreach (var key in server.KeysAsync(pattern: $"{fullPrefix}*").WithCancellation(ct))
            {
                batch.Add(key);
                keysToEvict.Add(key.ToString());

                if (batch.Count >= DeleteChunkSize)
                {
                    await db.KeyDeleteAsync(batch.ToArray()).ConfigureAwait(false);
                    await hybridCache.RemoveAsync(keysToEvict, ct).ConfigureAwait(false);
                    batch.Clear();
                    keysToEvict.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await db.KeyDeleteAsync(batch.ToArray()).ConfigureAwait(false);
                await hybridCache.RemoveAsync(keysToEvict, ct).ConfigureAwait(false);
            }
        }
    }

}
