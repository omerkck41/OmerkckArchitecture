using Core.BackgroundJobs.Interfaces;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Hangfire;
using Quartz;

namespace Core.BackgroundJobs.Services;

public class HangfireJobScheduler : IJobScheduler
{
    public async Task ScheduleRecurringJob<T>(string jobId, string cronExpression) where T : IJob
    {
        if (string.IsNullOrEmpty(jobId))
            throw new CustomArgumentException("Job ID cannot be null or empty.", nameof(jobId));
        if (string.IsNullOrEmpty(cronExpression))
            throw new CustomArgumentException("Cron expression cannot be null or empty.", nameof(cronExpression));

        RecurringJob.AddOrUpdate<T>(jobId, job => job.Execute(null!), cronExpression);
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