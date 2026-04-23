namespace Kck.BackgroundJobs.Abstractions;

/// <summary>
/// Provider-agnostic job scheduler for fire-and-forget, delayed, and recurring jobs.
/// </summary>
public interface IJobScheduler
{
    /// <summary>Enqueue a job for immediate execution.</summary>
    Task EnqueueAsync<TJob>(CancellationToken ct = default) where TJob : IBackgroundJob;

    /// <summary>Schedule a job for execution after a delay.</summary>
    Task ScheduleAsync<TJob>(TimeSpan delay, CancellationToken ct = default) where TJob : IBackgroundJob;

    /// <summary>Register or update a recurring job.</summary>
    Task AddOrUpdateRecurringAsync<TJob>(string jobId, string cronExpression, CancellationToken ct = default) where TJob : IBackgroundJob;

    /// <summary>Remove a recurring job.</summary>
    Task RemoveRecurringAsync(string jobId, CancellationToken ct = default);
}
