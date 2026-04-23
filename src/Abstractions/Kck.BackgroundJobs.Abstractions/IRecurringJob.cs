namespace Kck.BackgroundJobs.Abstractions;

/// <summary>
/// A recurring background job with a cron schedule.
/// </summary>
public interface IRecurringJob : IBackgroundJob
{
    /// <summary>Job identifier (unique per recurring job).</summary>
    string JobId { get; }

    /// <summary>Cron expression defining the schedule.</summary>
    string CronExpression { get; }
}
