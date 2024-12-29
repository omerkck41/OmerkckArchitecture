using Microsoft.Extensions.Logging;
using Quartz;

namespace Core.BackgroundJobs.Jobs;

public class DatabaseCleanupJob : IJob
{
    private readonly ILogger<DatabaseCleanupJob> _logger;

    public DatabaseCleanupJob(ILogger<DatabaseCleanupJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting database cleanup at {Time}", DateTime.Now);
        // Database cleanup logic here
        await Task.Delay(1000); // Simulated delay
        _logger.LogInformation("Database cleanup completed at {Time}", DateTime.Now);
    }
}