using Core.Api.Security.Config;
using Core.Api.Security.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Api.Security.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // CORS Yönetimi
        var allowedOrigins = configuration.GetSection("AddCorsPolicy").Get<string[]>();
        if (allowedOrigins != null)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        // IP Whitelist
        services.Configure<SecuritySettings>(options => configuration.GetSection("SecuritySettings").Bind(options));

        services.AddTransient<IpWhitelistMiddleware>();
        services.AddTransient<RateLimiterMiddleware>();
        services.AddTransient<HttpsEnforcerMiddleware>();
        services.AddTransient<SecurityHeadersMiddleware>();
        services.AddTransient<RequestValidationMiddleware>();
        services.AddTransient<AntiForgeryMiddleware>();
        services.AddTransient<BruteForceProtectionMiddleware>();

        return services;
    }

    public static IApplicationBuilder UseSecurityMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseMiddleware<IpWhitelistMiddleware>();
        app.UseMiddleware<RateLimiterMiddleware>();
        app.UseMiddleware<HttpsEnforcerMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<RequestValidationMiddleware>();
        app.UseMiddleware<AntiForgeryMiddleware>();
        app.UseMiddleware<BruteForceProtectionMiddleware>();

        var allowedOrigins = configuration.GetSection("AddCorsPolicy").Get<string[]>();
        if (allowedOrigins != null)
        {
            app.UseCors("DefaultCorsPolicy");
        }
        return app;
    }
}