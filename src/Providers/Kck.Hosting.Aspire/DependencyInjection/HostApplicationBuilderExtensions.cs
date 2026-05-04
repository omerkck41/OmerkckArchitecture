using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Aspire-oriented defaults for KCK services: service discovery, health checks,
/// and HTTP client default configuration.
/// </summary>
public static class KckAspireHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds KCK service defaults tuned for .NET Aspire:
    /// service discovery, health checks, and HTTP client service-discovery routing.
    /// </summary>
    /// <remarks>
    /// Call this before <c>AddKckObservability()</c> and <c>AddKckPipeline()</c>; those remain
    /// separate so projects not using Aspire are unaffected.
    /// </remarks>
    public static IHostApplicationBuilder AddKckServiceDefaults(
        this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
            http.AddServiceDiscovery());

        builder.Services.AddHealthChecks();

        return builder;
    }
}
