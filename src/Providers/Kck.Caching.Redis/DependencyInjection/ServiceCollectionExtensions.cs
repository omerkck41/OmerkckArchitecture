using Kck.Caching.Abstractions;
using Kck.Caching.Redis;
using Kck.Caching.Redis.DependencyInjection;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckCachingRedisServiceCollectionExtensions
{
    public static IServiceCollection AddKckCachingRedis(
        this IServiceCollection services,
        Action<RedisCacheOptions> configureRedis,
        Action<CacheOptions>? configureCaching = null)
    {
        var redisOptions = new RedisCacheOptions();
        configureRedis(redisOptions);

        services.AddStackExchangeRedisCache(configureRedis);

        services.TryAddSingleton<RedisConnectionHolder>();

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
                Kck.Caching.Redis configuration is incomplete. Set ONE of:
                  - opt.Configuration = "localhost:6379"
                  - opt.ConfigurationOptions = new ConfigurationOptions { ... }
                  - opt.ConnectionMultiplexerFactory = () => ConnectionMultiplexer.ConnectAsync(...)
                Example:
                  services.AddKckCachingRedis(opt => opt.Configuration = "localhost:6379");
                See: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/caching.md
                """);
        }

        services.TryAddSingleton(new RedisConnectionFactory(connector));
        services.AddHostedService<RedisConnectionHostedService>();

        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
            sp.GetRequiredService<RedisConnectionHolder>().Multiplexer);

        if (configureCaching is not null)
            services.Configure(configureCaching);
        else
            services.Configure<CacheOptions>(_ => { });

        services.TryAddSingleton<ICacheService, RedisCacheService>();
        return services;
    }
}
