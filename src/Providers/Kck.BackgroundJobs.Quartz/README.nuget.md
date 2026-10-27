# Kck.BackgroundJobs.Quartz

Quartz.NET-backed `IJobScheduler` implementation for fire-and-forget, delayed and cron-scheduled background jobs.

## Installation

```bash
dotnet add package Kck.BackgroundJobs.Quartz
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckBackgroundJobsQuartz();   // in-memory store, hosted service waits for running jobs on shutdown
builder.Services.AddKckJob<DailyReportJob>();     // jobs are resolved from DI in a new scope per execution

// Define a job
public sealed class DailyReportJob(IReportService reports) : IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default) => reports.GenerateDailyAsync(ct);
}

// Schedule it
public sealed class ReportSetup(IJobScheduler scheduler)
{
    public Task StartAsync(CancellationToken ct) =>
        scheduler.AddOrUpdateRecurringAsync<DailyReportJob>("daily-report", "0 0 8 * * ?", ct); // every day at 08:00
}
```

`IJobScheduler` also offers `EnqueueAsync<TJob>()` (run now), `ScheduleAsync<TJob>(delay)` and
`RemoveRecurringAsync(jobId)`.

## Configuration

Pass a Quartz.NET 4 builder callback for anything beyond the defaults (persistent store, thread pool, misfire
handling):

```csharp
builder.Services.AddKckBackgroundJobsQuartz(q =>
{
    q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10);
});
```

See the [Quartz.NET 4 documentation](https://www.quartz-scheduler.net/documentation/quartz-4.x/) for the builder API.
Quartz 4 changed some defaults (for example cron triggers without a time zone use `DefaultTimeZone`); check the
Quartz migration guide when upgrading from Kck 3.x.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/background-jobs.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
