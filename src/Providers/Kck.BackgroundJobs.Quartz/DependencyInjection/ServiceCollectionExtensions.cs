using Kck.BackgroundJobs.Abstractions;
using Kck.BackgroundJobs.Quartz;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckBackgroundJobsQuartzServiceCollectionExtensions
{
    public static IServiceCollection AddKckBackgroundJobsQuartz(
        this IServiceCollection services,
        Action<IServiceCollectionQuartzConfigurator>? configure = null)
    {
        services.AddQuartz(q =>
        {
            configure?.Invoke(q);
        });
        services.AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);
        services.TryAddSingleton<IJobScheduler, QuartzJobScheduler>();
        return services;
    }
}
