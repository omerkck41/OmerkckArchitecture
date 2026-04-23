using Hangfire;
using Hangfire.InMemory;
using Kck.BackgroundJobs.Abstractions;
using Kck.BackgroundJobs.Hangfire;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckBackgroundJobsHangfireServiceCollectionExtensions
{
    public static IServiceCollection AddKckBackgroundJobsHangfire(
        this IServiceCollection services,
        Action<HangfireOptions>? configure = null)
    {
        var options = new HangfireOptions();
        configure?.Invoke(options);

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();

            switch (options.StorageType.ToLowerInvariant())
            {
                case "inmemory":
                    config.UseInMemoryStorage();
                    break;
                default:
                    config.UseInMemoryStorage();
                    break;
            }
        });

        services.AddHangfireServer(serverOptions =>
        {
            serverOptions.WorkerCount = options.WorkerCount;
        });

        services.TryAddSingleton<IJobScheduler, HangfireJobScheduler>();
        return services;
    }
}
