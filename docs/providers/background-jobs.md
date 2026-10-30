# Background Jobs

A provider-agnostic abstraction for background job scheduling. The `IJobScheduler`
interface is implemented with the following technologies.

## Packages

| Package | Purpose |
|---|---|
| `Kck.BackgroundJobs.Abstractions` | `IJobScheduler`, `IBackgroundJob`, `IRecurringJob`, `CronExpressions` |
| `Kck.BackgroundJobs.Hangfire` | Hangfire (SQL Server / PostgreSQL / InMemory) |
| `Kck.BackgroundJobs.Quartz` | Quartz.NET (in-process scheduler) |

## Hangfire

```csharp
services.AddKckBackgroundJobsHangfire(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("Hangfire");
    options.ServerName = "api-worker-01";
});
```

For the dashboard:

```csharp
app.UseHangfireDashboard("/jobs");
```

> Note: `Hangfire.MySqlStorage` was removed because the package is abandoned
> ([ADR-0002](../adr/0002-hangfire-storage.md)). SQL Server, PostgreSQL, or
> InMemory are supported.

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
    public async Task HandleAsync()
    {
        await scheduler.EnqueueAsync<EmailCleanupJob>();
        await scheduler.ScheduleAsync<EmailCleanupJob>(TimeSpan.FromMinutes(5));
        await scheduler.AddOrUpdateRecurringAsync<EmailCleanupJob>(
            "daily-cleanup",
            CronExpressions.EveryDay);
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
| `EveryDay` | `0 0 * * *` |
| `EveryWeek` | `0 0 * * 1` |
| `EveryMonth` | `0 0 1 * *` |

## Selection Criteria

| Criterion | Hangfire | Quartz |
|---|---|---|
| Storage | SQL Server / PostgreSQL / Memory | In-process (persistence optional) |
| Dashboard | Built-in | Third-party |
| Distributed | Yes (worker pool) | Yes (clustering) |
| Cron | Cron + TimeSpan | Full Quartz cron |
| Best for | Web app job queue | Scheduler-heavy workload |
