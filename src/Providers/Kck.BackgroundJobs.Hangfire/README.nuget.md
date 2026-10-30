# Kck.BackgroundJobs.Hangfire

Hangfire-backed `IJobScheduler` implementation for reliable background job processing with persistent storage and a built-in dashboard.

## Installation

```bash
dotnet add package Kck.BackgroundJobs.Hangfire
```

## Quick Start

```csharp
using Hangfire;
using Kck.BackgroundJobs.Abstractions;

// Program.cs - in-memory storage by default; plug in a durable one with ConfigureStorage
builder.Services.AddKckBackgroundJobsHangfire(options =>
{
    options.WorkerCount = 4;
    // options.ConfigureStorage = config => config.UseSqlServerStorage(connectionString); // Hangfire.SqlServer (4.7.0+)
});
builder.Services.AddKckJob<WelcomeEmailJob>();

public sealed class WelcomeEmailJob(ILogger<WelcomeEmailJob> logger) : IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Sending welcome emails");
        return Task.CompletedTask;
    }
}

// Schedule a job (Hangfire accepts the 5-field CronExpressions constants)
public sealed class JobSetup(IJobScheduler scheduler)
{
    public async Task StartAsync(CancellationToken ct)
    {
        await scheduler.EnqueueAsync<WelcomeEmailJob>(ct);
        await scheduler.AddOrUpdateRecurringAsync<WelcomeEmailJob>("welcome", CronExpressions.EveryHour, ct);
    }
}

// The dashboard is in the Hangfire.AspNetCore package: app.UseHangfireDashboard("/jobs") with an authorization filter.
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `StorageType` | `InMemory` (the only built-in storage) | `"InMemory"` |
| `ConfigureStorage` | Plugs in another Hangfire storage, e.g. `c => c.UseSqlServerStorage(...)` (4.7.0+) | `null` |
| `WorkerCount` | Hangfire server worker count | `Environment.ProcessorCount` |

The dashboard is in `Hangfire.AspNetCore`, which this package does not reference.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/background-jobs.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
