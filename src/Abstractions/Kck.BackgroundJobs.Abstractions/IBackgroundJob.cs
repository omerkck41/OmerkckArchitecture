namespace Kck.BackgroundJobs.Abstractions;

/// <summary>
/// Marker interface for a background job that can be enqueued or scheduled.
/// Framework-agnostic alternative to Quartz.IJob and Hangfire's convention.
/// </summary>
public interface IBackgroundJob
{
    Task ExecuteAsync(CancellationToken ct = default);
}
