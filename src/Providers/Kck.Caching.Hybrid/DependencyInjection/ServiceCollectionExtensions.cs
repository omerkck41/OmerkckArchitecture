using Kck.Caching.Abstractions;
using Kck.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckCachingHybridServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="HybridCacheService"/> as <see cref="ICacheService"/>.
    /// L2 is backed by Redis; L1 is in-process memory managed by HybridCache.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureRedis">Redis connection and cache settings (same options as AddKckCachingRedis).</param>
    /// <param name="configureHybrid">Optional hybrid-specific settings (expiration, key prefix).</param>
    public static IServiceCollection AddKckCachingHybrid(
        this IServiceCollection services,
        Action<RedisCacheOptions> configureRedis,
        Action<HybridCacheOptions>? configureHybrid = null)
    {
        var redisOptions = new RedisCacheOptions();
        configureRedis(redisOptions);

        // L2: Redis as the distributed cache backing store for HybridCache
        services.AddStackExchangeRedisCache(configureRedis);

        // L1 + L2: Microsoft HybridCache (uses the registered IDistributedCache as L2)
        services.AddHybridCache();

        // IConnectionMultiplexer for ExistsAsync and RemoveByPrefixAsync
        services.TryAddSingleton<IConnectionMultiplexer>(_ =>
        {
            if (redisOptions.ConnectionMultiplexerFactory is not null)
                return redisOptions.ConnectionMultiplexerFactory().GetAwaiter().GetResult();

            if (!string.IsNullOrWhiteSpace(redisOptions.Configuration))
                return ConnectionMultiplexer.Connect(redisOptions.Configuration);

            if (redisOptions.ConfigurationOptions is not null)
                return ConnectionMultiplexer.Connect(redisOptions.ConfigurationOptions);

            throw new InvalidOperationException(
                """
                Kck.Caching.Hybrid: Redis connection is not configured. Set ONE of:
                  - opt.Configuration = "localhost:6379"
                  - opt.ConfigurationOptions = new ConfigurationOptions { ... }
                  - opt.ConnectionMultiplexerFactory = () => ConnectionMultiplexer.ConnectAsync(...)
                """);
        });

        if (configureHybrid is not null)
            services.Configure(configureHybrid);
        else
            services.Configure<HybridCacheOptions>(_ => { });

        services.TryAddSingleton<ICacheService, HybridCacheService>();

        return services;
    }
}
