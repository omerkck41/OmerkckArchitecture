using Kck.Bundle.WorkerService;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckBundleWorkerServiceCollectionExtensions
{
    /// <summary>
    /// Registers the opinionated Kck Worker Service stack: Serilog, OpenTelemetry,
    /// InMemory EventBus, and exactly one background-job provider (Hangfire or Quartz).
    /// </summary>
    /// <remarks>
    /// Configure the provider via <c>appsettings.json</c>:
    /// <code>
    /// "Kck": { "BackgroundJobs": { "Provider": "Hangfire" } }
    /// </code>
    /// Do not register both Hangfire and Quartz — pick one per host.
    /// </remarks>
    public static IServiceCollection AddKckWorkerServiceDefaults(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var defaults = configuration.GetSection("Kck").Get<KckWorkerServiceDefaults>()
                       ?? new KckWorkerServiceDefaults();

        // Logging
        services.AddKckSerilog();

        // Background Jobs — mutually exclusive
        switch (defaults.BackgroundJobs.Provider.ToUpperInvariant())
        {
            case "QUARTZ":
                services.AddKckBackgroundJobsQuartz();
                break;
            default:
                services.AddKckBackgroundJobsHangfire(opts =>
                    opts.WorkerCount = defaults.BackgroundJobs.HangfireWorkerCount);
                break;
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
