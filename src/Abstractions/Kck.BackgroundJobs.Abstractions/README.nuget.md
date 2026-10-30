# Kck.BackgroundJobs.Abstractions

Provider-agnostic background job scheduling abstractions — fire-and-forget, recurring, and delayed jobs without coupling to Hangfire or Quartz.

## Installation

```bash
dotnet add package Kck.BackgroundJobs.Abstractions
```

## Quick Start

```csharp
using Kck.BackgroundJobs.Abstractions;

// Program.cs — a provider (Kck.BackgroundJobs.Quartz or .Hangfire) and your jobs
builder.Services.AddKckBackgroundJobsQuartz();
builder.Services.AddKckJob<DailyReportJob>();

// A job: resolved from DI, so it can take dependencies
public class DailyReportJob(ILogger<DailyReportJob> logger) : IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Building the daily report");
        return Task.CompletedTask;
    }
}

// Schedule from anywhere
public class ReportScheduler(IJobScheduler scheduler)
{
    public async Task ScheduleAsync(CancellationToken ct)
    {
        await scheduler.EnqueueAsync<DailyReportJob>(ct);                                    // now
        await scheduler.ScheduleAsync<DailyReportJob>(TimeSpan.FromMinutes(5), ct);          // delayed
        // The cron format is the provider's: Quartz needs seconds first ("0 0 8 * * ?" = 08:00 daily);
        // the CronExpressions constants are 5-field crontab expressions for Hangfire.
        await scheduler.AddOrUpdateRecurringAsync<DailyReportJob>("daily-report", "0 0 8 * * ?", ct);
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
