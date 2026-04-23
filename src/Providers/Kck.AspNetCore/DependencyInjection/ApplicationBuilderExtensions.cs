using Kck.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckAspNetCoreApplicationBuilderExtensions
{
    public static IApplicationBuilder UseKckAspNetCore(this IApplicationBuilder app)
    {
        var builder = app.ApplicationServices.GetRequiredService<KckAspNetCoreBuilder>();

        if (builder.UseSecurityHeadersFlag)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
        }

        if (builder.UseRateLimitingFlag)
        {
            app.UseRateLimiter();
        }

        if (builder.CorsOrigins is not null)
        {
            if (builder.CorsOrigins.Length == 0)
            {
                var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
                if (env.IsProduction())
                {
                    var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("Kck.AspNetCore.Cors");
                    logger.LogWarning(
                        "CORS AllowAnyOrigin is active in Production. "
                        + "Configure explicit origins via UseCorsPolicy(origins) to restrict cross-origin access");
                }
            }

            app.UseCors(builder.CorsPolicyName);
        }

        return app;
    }
}
