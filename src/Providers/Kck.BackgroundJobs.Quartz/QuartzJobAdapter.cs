using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Kck.BackgroundJobs.Quartz;

/// <summary>
/// Adapts Kck IBackgroundJob to Quartz IJob.
/// </summary>
internal sealed class QuartzJobAdapter<TJob>(IServiceScopeFactory scopeFactory) : IJob
    where TJob : IBackgroundJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var job = scope.ServiceProvider.GetRequiredService<TJob>();
        await job.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
