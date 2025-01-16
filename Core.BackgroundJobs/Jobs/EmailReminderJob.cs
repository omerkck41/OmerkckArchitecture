using Quartz;

namespace Core.BackgroundJobs.Jobs;

public class EmailReminderJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Email sending logic here
        await Task.Delay(1000); // Simulated delay
    }
}