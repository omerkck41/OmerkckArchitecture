# Kck.BackgroundJobs.Abstractions

Provider-agnostic background job scheduling abstractions — fire-and-forget, recurring, and delayed jobs without coupling to Hangfire or Quartz.

## Installation

```bash
dotnet add package Kck.BackgroundJobs.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider (e.g. Kck.BackgroundJobs.Hangfire)
builder.Services.AddKckHangfireJobs(builder.Configuration);

// Define a job
public class SendWelcomeEmailJob(IEmailService email) : IJob
{
    public async Task ExecuteAsync(JobContext context, CancellationToken ct)
        => await email.SendAsync(new EmailMessage { To = context.GetArg<string>("email") }, ct);
}

// Enqueue from anywhere
public class UserRegistrationHandler(IJobScheduler scheduler)
{
    public async Task HandleAsync(string userEmail, CancellationToken ct)
    {
        // Fire-and-forget
        await scheduler.EnqueueAsync<SendWelcomeEmailJob>(
            args => args.Set("email", userEmail), ct);

        // Recurring (cron)
        await scheduler.AddRecurringAsync<SendWelcomeEmailJob>(
            "daily-report", "0 8 * * *", ct);

        // Delayed
        await scheduler.ScheduleAsync<SendWelcomeEmailJob>(
            args => args.Set("email", userEmail), TimeSpan.FromMinutes(5), ct);
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `BackgroundJobs:Provider` | `Hangfire` or `Quartz` | `Hangfire` |
| `BackgroundJobs:ConnectionString` | Storage connection (Hangfire SQL/Redis) | In-memory |
| `BackgroundJobs:WorkerCount` | Concurrent worker count | `5` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/background-jobs.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
