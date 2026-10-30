# Background Jobs

A provider-agnostic abstraction for background job scheduling. The `IJobScheduler`
interface is implemented with the following technologies.

## Packages

| Package | Purpose |
|---|---|
| `Kck.BackgroundJobs.Abstractions` | `IJobScheduler`, `IBackgroundJob`, `IRecurringJob`, `CronExpressions` |
| `Kck.BackgroundJobs.Hangfire` | Hangfire with in-memory storage |
| `Kck.BackgroundJobs.Quartz` | Quartz.NET (in-process scheduler) |

## Hangfire

```csharp
services.AddKckBackgroundJobsHangfire(options =>
{
    options.WorkerCount = 4;   // default: Environment.ProcessorCount
});
```

`AddKckBackgroundJobsHangfire` adds the Hangfire server and `IJobScheduler`. The only supported `StorageType` is
`InMemory` (the default): jobs do not survive a restart and are not shared between instances; any other value throws
`NotSupportedException`. `ConnectionString` is not used yet. For durable jobs, use Quartz with a persistent job store or
configure Hangfire storage yourself. ADR-0002 removed `Hangfire.MySqlStorage` (abandoned package).

The Hangfire dashboard is in the `Hangfire.AspNetCore` package, which this package does not reference; add it and call
`app.UseHangfireDashboard("/jobs")` with an authorization filter.

## Quartz

```csharp
services.AddKckBackgroundJobsQuartz();                         // defaults: in-memory store
services.AddKckBackgroundJobsQuartz(q => q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10)); // Quartz.NET 4 IQuartzBuilder
```

The Quartz hosted service is added automatically (`WaitForJobsToComplete = true`); jobs are resolved from DI. As of 4.0,
Quartz.NET 4 is used. For migration notes from Quartz 3, see [v3-to-v4](../migrations/v3-to-v4.md#7-quartznet-4).

> As of 4.0, `Kck.Bundle.WorkerService` includes only Quartz. For Hangfire (LGPL-3.0),
> add the `Kck.BackgroundJobs.Hangfire` package separately.

## Writing a Job

```csharp
using Kck.BackgroundJobs.Abstractions;

public class EmailCleanupJob : IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        // ...
        return Task.CompletedTask;
    }
}
```

For DI registration, use the helper from [ADR-0007](../adr/0007-add-kck-job-helper.md):

```csharp
services.AddKckJob<EmailCleanupJob>();
```

## Using the Scheduler

```csharp
public class CheckoutHandler(IJobScheduler scheduler)
{
    public async Task HandleAsync(CancellationToken ct)
    {
        await scheduler.EnqueueAsync<EmailCleanupJob>(ct);
        await scheduler.ScheduleAsync<EmailCleanupJob>(TimeSpan.FromMinutes(5), ct);
        await scheduler.AddOrUpdateRecurringAsync<EmailCleanupJob>("daily-cleanup", CronExpressions.EveryDay, ct);
    }
}
```

## Cron Constants

`CronExpressions` provides ready-made constants:

| Constant | Expression |
|---|---|
| `EveryMinute` | `* * * * *` |
| `EveryHour` | `0 * * * *` |
| `Every5Minutes` | `*/5 * * * *` |
| `Every15Minutes` | `*/15 * * * *` |
| `EveryDay` | `0 0 * * *` |
| `EveryWeek` | `0 0 * * 1` |
| `EveryMonth` | `0 0 1 * *` |

## Selection Criteria

| Criterion | Hangfire | Quartz |
|---|---|---|
| Storage | In-memory only (this package) | In-memory by default; persistent job store optional |
| Dashboard | `Hangfire.AspNetCore` (add it yourself) | Third-party |
| Distributed | No (in-memory storage) | Yes (clustering with a persistent store) |
| Cron | Cron + TimeSpan | Full Quartz cron |
| Best for | Web app job queue | Scheduler-heavy workload |
