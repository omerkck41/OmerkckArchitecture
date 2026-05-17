using Kck.Security.Abstractions.Token;
using Kck.Security.TokenBlacklist.Redis;
using Kck.Security.TokenBlacklist.Redis.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
        services.TryAddSingleton<IValidateOptions<RedisTokenBlacklistOptions>, RedisTokenBlacklistOptionsValidator>();
        services.AddOptions<RedisTokenBlacklistOptions>().ValidateOnStart();
        services.TryAddSingleton<ITokenBlacklistService, RedisTokenBlacklistService>();
        return services;
    }
}
