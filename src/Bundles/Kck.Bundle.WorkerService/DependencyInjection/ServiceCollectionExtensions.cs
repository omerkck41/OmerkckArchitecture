using Kck.Bundle.WorkerService;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckBundleWorkerServiceCollectionExtensions
{
    /// <summary>
    /// Registers the opinionated Kck Worker Service stack: Serilog, OpenTelemetry,
    /// InMemory EventBus, and the Quartz background-job scheduler.
    /// </summary>
    /// <remarks>
    /// Hangfire (LGPL-3.0) is not bundled. <c>"Kck": { "BackgroundJobs": { "Provider": "Hangfire" } }</c> fails at
    /// startup instead of silently switching schedulers; reference Kck.BackgroundJobs.Hangfire and call
    /// <c>AddKckBackgroundJobsHangfire</c> yourself if you need it.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The configured provider is not "Quartz".</exception>
    public static IServiceCollection AddKckWorkerServiceDefaults(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var defaults = configuration.GetSection("Kck").Get<KckWorkerServiceDefaults>()
                       ?? new KckWorkerServiceDefaults();

        // Logging
        services.AddKckSerilog();

        // Background Jobs — Quartz only; any other provider fails fast rather than silently changing the scheduler.
        switch (defaults.BackgroundJobs.Provider.ToUpperInvariant())
        {
            case "QUARTZ":
                services.AddKckBackgroundJobsQuartz();
                break;
            case "HANGFIRE":
                throw new InvalidOperationException(
                    """
                    Kck.Bundle.WorkerService 4.0 no longer bundles Hangfire (LGPL-3.0); Quartz is the bundled scheduler.
                      - Remove "Kck:BackgroundJobs:Provider" (or set it to "Quartz") to use Quartz, or
                      - keep Hangfire: add the Kck.BackgroundJobs.Hangfire package, call
                        services.AddKckBackgroundJobsHangfire(...) yourself and register the rest of the stack without
                        this bundle's scheduler. See docs/migrations/v3-to-v4.md.
                    """);
            default:
                throw new InvalidOperationException(
                    $"Kck.Bundle.WorkerService: unknown background job provider '{defaults.BackgroundJobs.Provider}'. " +
                    "Supported: \"Quartz\".");
        }

        // Event Bus
        services.AddKckEventBus(b => b.UseInMemory());

        // Observability
        services.AddKckObservability(obs =>
        {
            obs.UseHealthChecks(hc => { });
        });

        return services;
    }
}
