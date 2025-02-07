using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

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
    public static IServiceCollection AddJwtHelper<TUserId, TOperationClaimId, TRefreshTokenId>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TokenOptions> configureOptions = null,
        bool useRedis = false)
    {
        var tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>();
        configureOptions?.Invoke(tokenOptions);
        _ = services.AddSingleton(tokenOptions);

        // JWT Authentication yapılandırması
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidAudience = tokenOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey))
                };
            });

        var redisConfig = configuration.GetSection("Redis:Connection").Value;
        if (useRedis && !string.IsNullOrWhiteSpace(redisConfig))
        {
            var redisConnection = ConnectionMultiplexer.Connect(redisConfig);
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);
            services.AddSingleton<IDatabase>(sp => redisConnection.GetDatabase());
            services.AddSingleton<ITokenBlacklistManager, RedisTokenBlacklistManager>(); // Redis tabanlı blacklist yönetimi
        }
        else
        {
            services.AddSingleton<ITokenBlacklistManager, InMemoryTokenBlacklistManager>(); // Varsayılan olarak in-memory çalışacak
        }

        services.AddScoped<ITokenHelper<TUserId, TOperationClaimId, TRefreshTokenId>, JwtHelper<TUserId, TOperationClaimId, TRefreshTokenId>>();

        return services;
    }
}