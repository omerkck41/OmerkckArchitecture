# Kck.BackgroundJobs.Quartz

Quartz.NET-backed `IJobScheduler` implementation for cron-scheduled and trigger-based background job processing.

## Installation

```bash
dotnet add package Kck.BackgroundJobs.Quartz
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckQuartz(options =>
{
    options.UsePersistentStore = true;
    options.ConnectionString = builder.Configuration.GetConnectionString("Quartz")!;
});

// Define a job
public class DailyReportJob(IReportService reports) : IJob
{
    public async Task Execute(IJobExecutionContext context)
        => await reports.GenerateDailyAsync(context.CancellationToken);
}

// Register and schedule
builder.Services.AddKckQuartzJob<DailyReportJob>("0 0 8 * * ?"); // Every day at 08:00
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `UsePersistentStore` | Persist job state to database | `false` |
| `ConnectionString` | Database connection string (persistent mode) | `null` |
| `ThreadCount` | Quartz thread pool size | `10` |
| `MisfireThreshold` | Misfire threshold in milliseconds | `60000` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/background-jobs.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
