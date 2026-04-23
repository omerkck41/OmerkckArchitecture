using Kck.BackgroundJobs.Abstractions;
using Quartz;

namespace Kck.BackgroundJobs.Quartz;

public sealed class QuartzJobScheduler(ISchedulerFactory schedulerFactory) : IJobScheduler
{
    public async Task EnqueueAsync<TJob>(CancellationToken ct = default) where TJob : IBackgroundJob
    {
        var scheduler = await schedulerFactory.GetScheduler(ct).ConfigureAwait(false);
        var job = JobBuilder.Create<QuartzJobAdapter<TJob>>()
            .WithIdentity(Guid.CreateVersion7().ToString())
            .Build();
        var trigger = TriggerBuilder.Create()
            .StartNow()
            .Build();
        await scheduler.ScheduleJob(job, trigger, ct).ConfigureAwait(false);
    }

    public async Task ScheduleAsync<TJob>(TimeSpan delay, CancellationToken ct = default) where TJob : IBackgroundJob
    {
        var scheduler = await schedulerFactory.GetScheduler(ct).ConfigureAwait(false);
        var job = JobBuilder.Create<QuartzJobAdapter<TJob>>()
            .WithIdentity(Guid.CreateVersion7().ToString())
            .Build();
        var trigger = TriggerBuilder.Create()
            .StartAt(DateTimeOffset.UtcNow.Add(delay))
            .Build();
        await scheduler.ScheduleJob(job, trigger, ct).ConfigureAwait(false);
    }

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

    public async Task RemoveRecurringAsync(string jobId, CancellationToken ct = default)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct).ConfigureAwait(false);
        await scheduler.DeleteJob(new JobKey(jobId), ct).ConfigureAwait(false);
    }
}
