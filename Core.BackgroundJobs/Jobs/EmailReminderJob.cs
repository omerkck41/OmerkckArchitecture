using Microsoft.Extensions.Logging;
using Quartz;

namespace Core.BackgroundJobs.Jobs;

public class EmailReminderJob : IJob
{
    private readonly ILogger<EmailReminderJob> _logger;

    public EmailReminderJob(ILogger<EmailReminderJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Sending email reminders at {Time}", DateTime.Now);
        // Email sending logic here
        await Task.Delay(1000); // Simulated delay
        _logger.LogInformation("Email reminders sent at {Time}", DateTime.Now);
    }
}