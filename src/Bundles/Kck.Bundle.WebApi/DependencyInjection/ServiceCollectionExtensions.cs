using Kck.Bundle.WebApi;
using Kck.Exceptions.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckBundleWebApiServiceCollectionExtensions
{
    public static IServiceCollection AddKckWebApiDefaults(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var defaults = configuration.GetSection("Kck").Get<KckWebApiDefaults>() ?? new KckWebApiDefaults();

        // Logging
        services.AddKckSerilog();

        // Caching
        services.AddKckCachingInMemory();

        // Security — JWT
        if (defaults.Security.Jwt is { Issuer: not null })
        {
            services.AddKckJwt(jwt =>
            {
                jwt.Issuer = defaults.Security.Jwt.Issuer;
                jwt.Audience = defaults.Security.Jwt.Audience ?? string.Empty;
            });
        }

        // Security — Hashing
        services.AddKckArgon2();

        // Event Bus
        services.AddKckEventBus(b => b.UseInMemory());

        // Exception Handling
        services.AddKckExceptionHandling();

        // Observability
        services.AddKckObservability(obs =>
        {
            obs.UseHealthChecks(hc => { });
        });

        // ASP.NET Core
        services.AddKckAspNetCore(aspnet =>
        {
            aspnet.UseRateLimiting(rate =>
            {
                rate.PermitLimit = defaults.AspNetCore.RateLimitPermitLimit;
                rate.WindowInSeconds = defaults.AspNetCore.RateLimitWindowSeconds;
            });

            if (defaults.AspNetCore.SecurityHeaders)
                aspnet.UseSecurityHeaders();

            if (defaults.AspNetCore.CorsOrigins is { Length: > 0 })
                aspnet.UseCorsPolicy(defaults.AspNetCore.CorsOrigins);
        });

        return services;
    }
}
