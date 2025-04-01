using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

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
        Action<TokenOptions>? configureOptions = null,
        bool useRedis = false)
    {

        // `IOptions<TokenOptions>` kullanımı için yapılandırma
        services.Configure<TokenOptions>(configuration.GetSection("TokenOptions"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TokenOptions>>().Value);

        // JWT Authentication yapılandırması (Cookie ayarları kullanılmayacak)
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var tokenOptions = services.BuildServiceProvider().GetRequiredService<IOptions<TokenOptions>>().Value
                ?? throw new CustomInvalidOperationException("TokenOptions configuration not found.");

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

                // AJAX istekleri ve API tüketicileri için yönlendirme yerine 401 yanıtı ve kullanıcı dostu mesaj döndür
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        // Varsayılan challenge davranışını iptal ediyoruz
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = JsonSerializer.Serialize(new
                        {
                            success = false,
                            message = "Please login to access this page."
                        });

                        return context.Response.WriteAsync(response);
                    }
                };

            });

        var redisConfig = configuration.GetSection("Redis:Connection").Value;
        if (useRedis && !string.IsNullOrWhiteSpace(redisConfig))
        {
            var redisConnection = ConnectionMultiplexer.Connect(redisConfig);
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);
            services.AddSingleton<IDatabase>(sp => redisConnection.GetDatabase());
            services.AddSingleton<ITokenBlacklistManager<TUserId>, RedisTokenBlacklistManager<TUserId>>(); // Redis tabanlı blacklist yönetimi
        }
        else
        {
            services.AddSingleton<ITokenBlacklistManager<TUserId>, InMemoryTokenBlacklistManager<TUserId>>(); // Varsayılan olarak in-memory çalışacak
        }

        services.AddScoped<ITokenHelper<TUserId, TOperationClaimId, TRefreshTokenId>, JwtHelper<TUserId, TOperationClaimId, TRefreshTokenId>>();

        return services;
    }
}