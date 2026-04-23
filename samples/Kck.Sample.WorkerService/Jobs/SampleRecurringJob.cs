using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kck.Sample.WorkerService.Jobs;

public sealed class SampleRecurringJob(ILogger<SampleRecurringJob> logger) : IRecurringJob
{
    public string JobId => "sample-recurring";
    public string CronExpression => CronExpressions.Every5Minutes;

    public Task ExecuteAsync(CancellationToken ct = default)
    {
        logger.LogInformation("SampleRecurringJob executed at {Time}", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}
