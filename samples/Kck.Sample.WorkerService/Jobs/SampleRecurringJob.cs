using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kck.Sample.WorkerService.Jobs;

public sealed partial class SampleRecurringJob(ILogger<SampleRecurringJob> logger) : IRecurringJob
{
    public string JobId => "sample-recurring";
    public string CronExpression => CronExpressions.Every5Minutes;

    public Task ExecuteAsync(CancellationToken ct = default)
    {
        LogExecuted(logger, DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "SampleRecurringJob executed at {Time}")]
    private static partial void LogExecuted(ILogger logger, DateTimeOffset time);
}
