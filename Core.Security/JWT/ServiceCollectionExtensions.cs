using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Core.Security.JWT;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// JWT ve Redis tabanlı token hizmetlerini yapılandırır.
    /// </summary>
    /// <typeparam name="TUserId">Kullanıcı ID türü.</typeparam>
    /// <typeparam name="TOperationClaimId">Operasyon yetkisi ID türü.</typeparam>
    /// <typeparam name="TRefreshTokenId">Refresh token ID türü.</typeparam>
    /// <param name="services">DI konteyner hizmetleri.</param>
    /// <param name="configuration">Uygulama yapılandırması.</param>
    /// <param name="configureOptions">Token yapılandırma seçenekleri (isteğe bağlı).</param>
    /// <param name="useRedis">Redis kullanım tercihi.</param>
    /// <returns>Yapılandırılmış hizmet koleksiyonu.</returns>
    public static IServiceCollection AddJwtHelper<TUserId, TOperationClaimId, TRefreshTokenId>(this IServiceCollection services, IConfiguration configuration, Action<TokenOptions> configureOptions = null, bool useRedis = false)
    {
        var tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>();

        configureOptions?.Invoke(tokenOptions);

        services.AddSingleton(tokenOptions);

        if (useRedis)
        {
            var redisConnection = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis"));
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);
            services.AddSingleton<IServer>(redisConnection.GetServer(redisConnection.GetEndPoints().First())); // IServer ekleniyor
            services.AddSingleton<ITokenBlacklist, RedisTokenBlacklist>();
            services.AddScoped<IRefreshTokenRepository<TRefreshTokenId, TUserId>, RedisRefreshTokenRepository<TRefreshTokenId, TUserId>>();
        }
        else
        {
            services.AddSingleton<TokenBlacklist<int>>();
        }

        services.AddScoped<ITokenHelper<TUserId, TOperationClaimId, TRefreshTokenId>, JwtHelper<TUserId, TOperationClaimId, TRefreshTokenId>>();

        return services;
    }
}