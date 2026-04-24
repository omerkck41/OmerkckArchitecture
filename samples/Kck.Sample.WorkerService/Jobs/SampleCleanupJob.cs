using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kck.Sample.WorkerService.Jobs;

public sealed partial class SampleCleanupJob(ILogger<SampleCleanupJob> logger) : IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        LogExecuted(logger, DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "SampleCleanupJob executed at {Time}")]
    private static partial void LogExecuted(ILogger logger, DateTimeOffset time);
}
