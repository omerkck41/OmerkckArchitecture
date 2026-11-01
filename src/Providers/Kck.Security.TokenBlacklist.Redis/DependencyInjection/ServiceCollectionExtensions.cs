using Kck.Security.Abstractions.Token;
using Kck.Security.TokenBlacklist.Redis;
using Kck.Security.TokenBlacklist.Redis.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSecurityTokenBlacklistRedisServiceCollectionExtensions
{
    /// <summary>Adds Redis-based token blacklist service.</summary>
    /// <remarks>
    /// With <c>ConnectionString</c> set, the blacklist opens its own connection asynchronously on first use (a failed
    /// connect is retried on the next call). Left empty, it shares the registered <see cref="IConnectionMultiplexer"/>;
    /// with Kck.Caching.Redis (<c>AddKckCachingRedis</c>) that connection is available after the host started.
    /// </remarks>
    public static IServiceCollection AddKckTokenBlacklistRedis(
        this IServiceCollection services,
        Action<RedisTokenBlacklistOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<RedisTokenBlacklistOptions>, RedisTokenBlacklistOptionsValidator>();
        services.AddOptions<RedisTokenBlacklistOptions>().ValidateOnStart();
        services.TryAddSingleton<ITokenBlacklistService>(sp =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<RedisTokenBlacklistOptions>>();
            return string.IsNullOrWhiteSpace(options.CurrentValue.ConnectionString)
                ? new RedisTokenBlacklistService(options, sp.GetRequiredService<IConnectionMultiplexer>())
                : new RedisTokenBlacklistService(options);
        });
        return services;
    }
}
