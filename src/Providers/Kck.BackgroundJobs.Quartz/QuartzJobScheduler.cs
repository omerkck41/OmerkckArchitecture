using Kck.BackgroundJobs.Abstractions;
using Quartz;

namespace Kck.BackgroundJobs.Quartz;

/// <summary>
/// Quartz.NET-backed implementation of <see cref="IJobScheduler"/> that enqueues, schedules and
/// manages recurring background jobs through the Quartz scheduler factory.
/// </summary>
public sealed class QuartzJobScheduler(ISchedulerFactory schedulerFactory) : IJobScheduler
{
    private static Guid NewJobId() => Guid.CreateVersion7();

    /// <summary>
    /// Enqueues a one-time fire-and-forget execution of <typeparamref name="TJob"/> that starts
    /// immediately.
    /// </summary>
    public async Task EnqueueAsync<TJob>(CancellationToken ct = default) where TJob : IBackgroundJob
    {
        var scheduler = await schedulerFactory.GetScheduler(ct).ConfigureAwait(false);
        var job = JobBuilder.Create<QuartzJobAdapter<TJob>>()
            .WithIdentity(NewJobId().ToString())
            .Build();
        var trigger = TriggerBuilder.Create()
            .StartNow()
            .Build();
        await scheduler.ScheduleJob(job, trigger, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Schedules a one-time execution of <typeparamref name="TJob"/> to fire after the specified
    /// <paramref name="delay"/>.
    /// </summary>
    public async Task ScheduleAsync<TJob>(TimeSpan delay, CancellationToken ct = default) where TJob : IBackgroundJob
    {
        var scheduler = await schedulerFactory.GetScheduler(ct).ConfigureAwait(false);
        var job = JobBuilder.Create<QuartzJobAdapter<TJob>>()
            .WithIdentity(NewJobId().ToString())
            .Build();
        var trigger = TriggerBuilder.Create()
            .StartAt(DateTimeOffset.UtcNow.Add(delay))
            .Build();
        await scheduler.ScheduleJob(job, trigger, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates or replaces a recurring Quartz job identified by <paramref name="jobId"/> using the
    /// given Cron <paramref name="cronExpression"/>.
    /// </summary>
    public async Task AddOrUpdateRecurringAsync<TJob>(string jobId, string cronExpression, CancellationToken ct = default)
        where TJob : IBackgroundJob
    {
        var scheduler = await schedulerFactory.GetScheduler(ct).ConfigureAwait(false);
        var jobKey = new JobKey(jobId);

        var job = JobBuilder.Create<QuartzJobAdapter<TJob>>()
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobId}-trigger")
            .WithCronSchedule(cronExpression)
            .Build();

        if (await scheduler.CheckExists(jobKey, ct).ConfigureAwait(false))
            await scheduler.DeleteJob(jobKey, ct).ConfigureAwait(false);

        await scheduler.ScheduleJob(job, trigger, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the recurring job identified by <paramref name="jobId"/> from the Quartz scheduler.
    /// </summary>
    public async Task RemoveRecurringAsync(string jobId, CancellationToken ct = default)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct).ConfigureAwait(false);
        await scheduler.DeleteJob(new JobKey(jobId), ct).ConfigureAwait(false);
    }
}
