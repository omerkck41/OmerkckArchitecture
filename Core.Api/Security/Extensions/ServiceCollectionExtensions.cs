using Core.Api.Security.Config;
using Core.Api.Security.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Api.Security.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // CORS Yönetimi
        var allowedOrigins = configuration.GetSection("AddCorsPolicy").Get<string[]>() ?? new string[0];
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", builder =>
            {
                builder.WithOrigins(allowedOrigins)
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        // SecuritySettings yapılandırmasını ekleyelim
        services.Configure<SecuritySettings>(options => configuration.GetSection("SecuritySettings").Bind(options));

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

        app.UseCors("DefaultCorsPolicy");

        return app;
    }
}