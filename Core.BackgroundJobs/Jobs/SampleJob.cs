using Quartz;

namespace Core.BackgroundJobs.Jobs;

public class SampleJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await Task.Delay(1000); // Simulated delay
    }
}