using Quartz;

namespace Core.BackgroundJobs.Interfaces;

/// <summary>
/// Interface for job scheduling with recurring, delayed, and immediate job options.
/// </summary>
public interface IJobScheduler
{
    Task ScheduleRecurringJob<T>(string jobId, string cronExpression) where T : IJob;
    Task ScheduleDelayedJob<T>(string jobId, TimeSpan delay) where T : IJob;
    Task EnqueueJob<T>() where T : IJob;
}
