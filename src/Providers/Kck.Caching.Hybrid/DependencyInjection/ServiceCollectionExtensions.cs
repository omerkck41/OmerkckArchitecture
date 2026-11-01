using Kck.Caching.Abstractions;
using Kck.Caching.Hybrid;
using Kck.Caching.Hybrid.DependencyInjection;
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
    /// <remarks>
    /// The Redis connection is opened asynchronously when the host starts (same as Kck.Caching.Redis); resolving
    /// <see cref="ICacheService"/> before the host's <c>StartAsync</c> completed throws.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configureRedis">Redis connection and cache settings (same options as AddKckCachingRedis).</param>
    /// <param name="configureHybrid">Optional hybrid-specific settings (expiration, key prefix).</param>
    /// <exception cref="InvalidOperationException">No Redis connection is configured in <paramref name="configureRedis"/>.</exception>
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

        // IConnectionMultiplexer for ExistsAsync and RemoveByPrefixAsync: opened asynchronously by a hosted service at
        // host start (same pattern as Kck.Caching.Redis), never inside DI resolution.
        Func<CancellationToken, Task<IConnectionMultiplexer>> connector;
        if (redisOptions.ConnectionMultiplexerFactory is not null)
        {
            var userFactory = redisOptions.ConnectionMultiplexerFactory;
            connector = _ => userFactory();
        }
        else if (!string.IsNullOrWhiteSpace(redisOptions.Configuration))
        {
            var configuration = redisOptions.Configuration;
            connector = async _ => await ConnectionMultiplexer.ConnectAsync(configuration).ConfigureAwait(false);
        }
        else if (redisOptions.ConfigurationOptions is not null)
        {
            var configurationOptions = redisOptions.ConfigurationOptions;
            connector = async _ => await ConnectionMultiplexer.ConnectAsync(configurationOptions).ConfigureAwait(false);
        }
        else
        {
            throw new InvalidOperationException(
                """
                Kck.Caching.Hybrid: Redis connection is not configured. Set ONE of:
                  - opt.Configuration = "localhost:6379"
                  - opt.ConfigurationOptions = new ConfigurationOptions { ... }
                  - opt.ConnectionMultiplexerFactory = () => ConnectionMultiplexer.ConnectAsync(...)
                """);
        }

        services.TryAddSingleton<HybridRedisConnectionHolder>();
        services.TryAddSingleton(new HybridRedisConnectionFactory(connector));
        services.AddHostedService<HybridRedisConnectionHostedService>();
        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
            sp.GetRequiredService<HybridRedisConnectionHolder>().Multiplexer);

        if (configureHybrid is not null)
            services.Configure(configureHybrid);
        else
            services.Configure<HybridCacheOptions>(_ => { });

        services.TryAddSingleton<ICacheService, HybridCacheService>();

        return services;
    }
}
