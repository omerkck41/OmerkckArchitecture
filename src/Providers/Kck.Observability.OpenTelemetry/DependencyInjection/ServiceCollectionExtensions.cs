using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckObservabilityServiceCollectionExtensions
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    public static IServiceCollection AddKckObservability(
        this IServiceCollection services,
        Action<KckObservabilityBuilder> configure)
    {
        var builder = new KckObservabilityBuilder(services);
        configure(builder);
        return services;
    }

    /// <summary>
    /// Maps K8s-compatible health check endpoints:
    /// <list type="bullet">
    ///   <item><c>/health/live</c> — Liveness: process alive check</item>
    ///   <item><c>/health/ready</c> — Readiness: dependency availability check</item>
    ///   <item><c>/health/startup</c> — Startup: initial boot completion check</item>
    /// </list>
    /// Checks without tags are included in all endpoints.
    /// </summary>
    public static IApplicationBuilder MapKckHealthChecks(
        this IApplicationBuilder app,
        string basePath = "/health")
    {
        app.UseHealthChecks($"{basePath}/live", new HealthCheckOptions
        {
            Predicate = reg => reg.Tags.Contains(KckHealthChecksBuilder.LiveTag) || reg.Tags.Count == 0,
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks($"{basePath}/ready", new HealthCheckOptions
        {
            Predicate = reg => reg.Tags.Contains(KckHealthChecksBuilder.ReadyTag) || reg.Tags.Count == 0,
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks($"{basePath}/startup", new HealthCheckOptions
        {
            Predicate = reg => reg.Tags.Contains(KckHealthChecksBuilder.StartupTag) || reg.Tags.Count == 0,
            ResponseWriter = WriteHealthCheckResponse
        });

        // Keep aggregate endpoint for backward compatibility
        app.UseHealthChecks(basePath, new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(
        Microsoft.AspNetCore.Http.HttpContext context,
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        await System.Text.Json.JsonSerializer.SerializeAsync(
            context.Response.Body, result, JsonOptions);
    }
}
