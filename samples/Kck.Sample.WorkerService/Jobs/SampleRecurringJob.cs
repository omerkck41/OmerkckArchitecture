using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kck.Sample.WorkerService.Jobs;

// Scheduled in Program.cs with IJobScheduler.AddOrUpdateRecurringAsync<SampleRecurringJob>(jobId, cron).
public sealed partial class SampleRecurringJob(ILogger<SampleRecurringJob> logger) : IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        LogExecuted(logger, DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "SampleRecurringJob executed at {Time}")]
    private static partial void LogExecuted(ILogger logger, DateTimeOffset time);
}
