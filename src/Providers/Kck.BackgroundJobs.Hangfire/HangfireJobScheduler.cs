using Hangfire;
using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Kck.BackgroundJobs.Hangfire;

public sealed class HangfireJobScheduler(IServiceScopeFactory scopeFactory) : IJobScheduler
{
    public Task EnqueueAsync<TJob>(CancellationToken ct = default) where TJob : IBackgroundJob
    {
        BackgroundJob.Enqueue(() => ExecuteJob<TJob>(CancellationToken.None));
        return Task.CompletedTask;
    }

    public Task ScheduleAsync<TJob>(TimeSpan delay, CancellationToken ct = default) where TJob : IBackgroundJob
    {
        BackgroundJob.Schedule(() => ExecuteJob<TJob>(CancellationToken.None), delay);
        return Task.CompletedTask;
    }

    public Task AddOrUpdateRecurringAsync<TJob>(string jobId, string cronExpression, CancellationToken ct = default)
        where TJob : IBackgroundJob
    {
        RecurringJob.AddOrUpdate(jobId, () => ExecuteJob<TJob>(CancellationToken.None), cronExpression);
        return Task.CompletedTask;
    }

    public Task RemoveRecurringAsync(string jobId, CancellationToken ct = default)
    {
        RecurringJob.RemoveIfExists(jobId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called by Hangfire via serialized method expression.
    /// Must be public for Hangfire serialization.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteJob<TJob>(CancellationToken ct) where TJob : IBackgroundJob
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var job = scope.ServiceProvider.GetRequiredService<TJob>();
        await job.ExecuteAsync(ct).ConfigureAwait(false);
    }
}
