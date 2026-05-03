using Kck.Bundle.MinimalApi;
using Kck.Exceptions.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckBundleMinimalApiServiceCollectionExtensions
{
    /// <summary>
    /// Registers the lightweight Kck Minimal API stack: Serilog, InMemory cache,
    /// JWT authentication, exception handling, and OpenTelemetry health checks.
    /// MVC, FluentValidation auto-wire, and Pipeline.MediatR are intentionally excluded.
    /// </summary>
    /// <remarks>
    /// Configure via <c>appsettings.json</c>:
    /// <code>
    /// "Kck": { "Security": { "Jwt": { "Issuer": "https://..." } } }
    /// </code>
    /// </remarks>
    public static IServiceCollection AddKckMinimalApiDefaults(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var defaults = configuration.GetSection("Kck").Get<KckMinimalApiDefaults>()
                       ?? new KckMinimalApiDefaults();

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

        // Exception Handling
        services.AddKckExceptionHandling();

        // Observability
        services.AddKckObservability(obs =>
        {
            obs.UseHealthChecks(hc => { });
        });

        return services;
    }
}
