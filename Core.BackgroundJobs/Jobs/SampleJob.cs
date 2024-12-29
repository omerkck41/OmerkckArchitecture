using Microsoft.Extensions.Logging;
using Quartz;

namespace Core.BackgroundJobs.Jobs;

public class SampleJob : IJob
{
    private readonly ILogger<SampleJob> _logger;

    public SampleJob(ILogger<SampleJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Executing SampleJob at {Time}", DateTime.Now);
        await Task.Delay(1000); // Simulated delay
        _logger.LogInformation("SampleJob completed at {Time}", DateTime.Now);
    }
}