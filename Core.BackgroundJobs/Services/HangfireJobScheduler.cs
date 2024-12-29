using Core.BackgroundJobs.Interfaces;
using Hangfire;
using Quartz;

namespace Core.BackgroundJobs.Services;

public class HangfireJobScheduler : IJobScheduler
{
    public async Task ScheduleRecurringJob<T>(string jobId, string cronExpression) where T : IJob
    {
        RecurringJob.AddOrUpdate<T>(jobId, job => job.Execute(null!), cronExpression); // null! operatörü ile null olduğu belirtiliyor
        await Task.CompletedTask;
    }

    public async Task ScheduleDelayedJob<T>(string jobId, TimeSpan delay) where T : IJob
    {
        BackgroundJob.Schedule<T>(job => job.Execute(null!), delay); // null! operatörü kullanıldı
        await Task.CompletedTask;
    }

    public async Task EnqueueJob<T>() where T : IJob
    {
        BackgroundJob.Enqueue<T>(job => job.Execute(null!)); // null! operatörü kullanıldı
        await Task.CompletedTask;
    }
}