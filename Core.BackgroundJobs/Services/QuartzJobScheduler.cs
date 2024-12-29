using Core.BackgroundJobs.Interfaces;
using Quartz;
using Quartz.Impl;

namespace Core.BackgroundJobs.Services;

public class QuartzJobScheduler : IJobScheduler
{
    private readonly IScheduler _scheduler;

    public QuartzJobScheduler()
    {
        _scheduler = new StdSchedulerFactory().GetScheduler().Result;
    }

    public async Task ScheduleRecurringJob<T>(string jobId, string cronExpression) where T : IJob
    {
        var job = JobBuilder.Create<T>().WithIdentity(jobId).Build();
        var trigger = TriggerBuilder.Create().WithCronSchedule(cronExpression).Build();

        await _scheduler.ScheduleJob(job, trigger);
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
