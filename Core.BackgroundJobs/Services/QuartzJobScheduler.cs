using Core.BackgroundJobs.Interfaces;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Quartz;

namespace Core.BackgroundJobs.Services;

public class QuartzJobScheduler : IJobScheduler
{
    private readonly IScheduler _scheduler;

    public QuartzJobScheduler(IScheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    public async Task ScheduleRecurringJob<T>(string jobId, string cronExpression) where T : IJob
    {
        if (string.IsNullOrEmpty(jobId))
            throw new CustomArgumentException("Job ID cannot be null or empty.", nameof(jobId));
        if (string.IsNullOrEmpty(cronExpression))
            throw new CustomArgumentException("Cron expression cannot be null or empty.", nameof(cronExpression));

        var job = JobBuilder.Create<T>().WithIdentity(jobId).Build();
        var trigger = TriggerBuilder.Create().WithCronSchedule(cronExpression).Build();

        await _scheduler.ScheduleJob(job, trigger);
        if (!_scheduler.IsStarted)
            await _scheduler.Start();
    }

    public async Task ScheduleDelayedJob<T>(string jobId, TimeSpan delay) where T : IJob
    {
        var job = JobBuilder.Create<T>().WithIdentity(jobId).Build();
        var trigger = TriggerBuilder.Create().StartAt(DateTimeOffset.Now.Add(delay)).Build();

        await _scheduler.ScheduleJob(job, trigger);
        await _scheduler.Start();
    }

    public async Task EnqueueJob<T>() where T : IJob
    {
        var job = JobBuilder.Create<T>().Build();
        await _scheduler.AddJob(job, true);
        await _scheduler.Start();
    }
}
