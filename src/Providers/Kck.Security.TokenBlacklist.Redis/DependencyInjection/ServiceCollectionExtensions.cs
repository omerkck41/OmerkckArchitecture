using Kck.Security.Abstractions.Token;
using Kck.Security.TokenBlacklist.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSecurityTokenBlacklistRedisServiceCollectionExtensions
{
    /// <summary>Adds Redis-based token blacklist service.</summary>
    public static IServiceCollection AddKckTokenBlacklistRedis(
        this IServiceCollection services,
        Action<RedisTokenBlacklistOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.TryAddSingleton<ITokenBlacklistService, RedisTokenBlacklistService>();
        return services;
    }
}
