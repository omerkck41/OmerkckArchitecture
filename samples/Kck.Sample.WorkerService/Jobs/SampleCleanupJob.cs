using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kck.Sample.WorkerService.Jobs;

public sealed class SampleCleanupJob(ILogger<SampleCleanupJob> logger) : IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        logger.LogInformation("SampleCleanupJob executed at {Time}", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}
