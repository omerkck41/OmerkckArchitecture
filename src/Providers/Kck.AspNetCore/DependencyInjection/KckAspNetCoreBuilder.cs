using System.Threading.RateLimiting;
using Kck.AspNetCore.Sanitization;
using Kck.AspNetCore.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public sealed class KckAspNetCoreBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
    internal bool UseSecurityHeadersFlag { get; private set; }
    internal bool UseRateLimitingFlag { get; private set; }
    internal string[]? CorsOrigins { get; private set; }
    internal string CorsPolicyName { get; private set; } = "KckCorsPolicy";

    public KckAspNetCoreBuilder UseRateLimiting(Action<KckRateLimitOptions>? configure = null)
    {
        var options = new KckRateLimitOptions();
        configure?.Invoke(options);

        Services.AddRateLimiter(limiter =>
        {
            limiter.GlobalLimiter = PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(
                context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowInSeconds),
                        QueueLimit = 0
                    });
                });

            limiter.RejectionStatusCode = 429;
        });

        UseRateLimitingFlag = true;
        return this;
    }

    public KckAspNetCoreBuilder UseSecurityHeaders()
    {
        UseSecurityHeadersFlag = true;
        return this;
    }

    /// <summary>
    /// Configures CORS policy. When <paramref name="origins"/> is null or empty,
    /// AllowAnyOrigin is used which is only safe for development.
    /// A warning is logged at startup if AllowAnyOrigin is active in Production.
    /// </summary>
    public KckAspNetCoreBuilder UseCorsPolicy(string[]? origins = null, string policyName = "KckCorsPolicy")
    {
        CorsOrigins = origins ?? [];
        CorsPolicyName = policyName;

        Services.AddCors(cors =>
        {
            cors.AddPolicy(policyName, policy =>
            {
                if (origins is { Length: > 0 })
                {
                    policy.WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
                else
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
            });
        });

        return this;
    }

    public KckAspNetCoreBuilder AddInputSanitizer()
    {
        Services.TryAddSingleton<IInputSanitizer, InputSanitizer>();
        return this;
    }

    public KckAspNetCoreBuilder AddCookieManager()
    {
        Services.TryAddSingleton<ICookieManager, CookieManager>();
        return this;
    }

    public KckAspNetCoreBuilder AddSessionManager()
    {
        Services.TryAddSingleton<ISessionManager, SessionManager>();
        return this;
    }
}

public sealed class KckRateLimitOptions
{
    public int PermitLimit { get; set; } = 100;
    public int WindowInSeconds { get; set; } = 60;
}
