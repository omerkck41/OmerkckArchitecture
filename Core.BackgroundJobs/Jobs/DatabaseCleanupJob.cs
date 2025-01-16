using Quartz;

namespace Core.BackgroundJobs.Jobs;

public class DatabaseCleanupJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Database cleanup logic here
        await Task.Delay(1000); // Simulated delay
    }
}