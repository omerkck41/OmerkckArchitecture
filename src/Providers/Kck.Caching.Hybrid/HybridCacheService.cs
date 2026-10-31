using System.Diagnostics;
using Kck.Caching.Abstractions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;
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
    IConnectionMultiplexer redis,
    IOptionsMonitor<HybridCacheOptions> options) : CacheServiceBase
{
    private readonly HybridCacheOptions _opts = options.CurrentValue;

    // The L2 RedisCache stores each key as InstanceName + key; HybridCache's L1 uses the key alone. ExistsAsync and
    // RemoveByPrefixAsync talk to Redis directly, so they add the name there and strip it again for L1.
    private readonly string _instanceName = string.Empty;

    /// <summary>
    /// The constructor the container uses: <paramref name="redisOptions"/> supplies the <c>InstanceName</c> that the
    /// L2 Redis cache prepends to every key.
    /// </summary>
    public HybridCacheService(
        HybridCache hybridCache,
        IConnectionMultiplexer redis,
        IOptionsMonitor<HybridCacheOptions> options,
        IOptions<RedisCacheOptions> redisOptions)
        : this(hybridCache, redis, options)
    {
        ArgumentNullException.ThrowIfNull(redisOptions);
        _instanceName = redisOptions.Value.InstanceName ?? string.Empty;
    }

    protected override CacheOptions Options { get; } = new CacheOptions
    {
        KeyPrefix = options.CurrentValue.KeyPrefix,
        DefaultExpiration = options.CurrentValue.DefaultExpiration
    };

    // Read-only lookup: HybridCache returns a stored value (L1, then L2) but never stores the factory result.
    private static readonly HybridCacheEntryOptions ReadOnlyEntry = new()
    {
        Flags = HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite
    };

    /// <summary>
    /// Retrieves an entry through HybridCache (L1, then L2) without populating either level on a miss.
    /// L2 entries are written by HybridCache in its own payload format, so they must be read through it too.
    /// </summary>
    public override async ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default)
        where T : default
    {
        return await hybridCache.GetOrCreateAsync<T?>(
                BuildKey(key), static _ => ValueTask.FromResult<T?>(default), ReadOnlyEntry, cancellationToken: ct)
            .ConfigureAwait(false);
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
        return await redis.GetDatabase().KeyExistsAsync(_instanceName + BuildKey(key)).ConfigureAwait(false);
    }

    private const int DeleteChunkSize = 1000;

    /// <summary>
    /// Removes all keys matching the prefix from L2 (Redis SCAN) and evicts
    /// each from L1 via HybridCache.RemoveAsync.
    /// </summary>
    public override async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var fullPrefix = _instanceName + BuildKey(prefix);
        var db = redis.GetDatabase();

        foreach (var server in redis.GetEndPoints().Select(e => redis.GetServer(e)))
        {
            var batch = new List<RedisKey>(DeleteChunkSize);
            var keysToEvict = new List<string>(DeleteChunkSize);

            await foreach (var key in server.KeysAsync(pattern: $"{fullPrefix}*").WithCancellation(ct))
            {
                batch.Add(key);
                keysToEvict.Add(key.ToString()[_instanceName.Length..]);

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
