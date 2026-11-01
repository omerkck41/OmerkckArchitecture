# Background Jobs

A provider-agnostic abstraction for background job scheduling. The `IJobScheduler`
interface is implemented with the following technologies.

## Packages

| Package | Purpose |
|---|---|
| `Kck.BackgroundJobs.Abstractions` | `IJobScheduler`, `IBackgroundJob`, `CronExpressions` |
| `Kck.BackgroundJobs.Hangfire` | Hangfire with in-memory storage |
| `Kck.BackgroundJobs.Quartz` | Quartz.NET (in-process scheduler) |

## Hangfire

```csharp
services.AddKckBackgroundJobsHangfire(options =>
{
    options.WorkerCount = 4;   // default: Environment.ProcessorCount
});
```

`AddKckBackgroundJobsHangfire` adds the Hangfire server and `IJobScheduler`. Without further configuration the storage
is in memory: jobs do not survive a restart and are not shared between instances. For durable jobs, add a Hangfire
storage package and plug it in with `ConfigureStorage` (4.7.0):

```csharp
using Hangfire;

services.AddKckBackgroundJobsHangfire(options =>
    options.ConfigureStorage = config => config.UseInMemoryStorage()); // e.g. config.UseSqlServerStorage(connectionString)
```

When `ConfigureStorage` is set, `StorageType` is ignored; otherwise only `InMemory` is accepted. 5.0 removed
`ConnectionString` (obsolete since 4.7.0, KCK0700: it was never read). ADR-0002 removed `Hangfire.MySqlStorage`
(abandoned package).

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
        // A 5-field crontab constant works with both providers (Quartz converts it).
        await scheduler.AddOrUpdateRecurringAsync<EmailCleanupJob>("daily-cleanup", CronExpressions.EveryDay, ct);
    }
}
```

A recurring job is an ordinary `IBackgroundJob`: the job id and the cron expression are arguments of
`AddOrUpdateRecurringAsync`. (5.0 removed `IRecurringJob`, which declared them as properties but was never read.)

## Cron Constants

`CronExpressions` provides ready-made constants in the 5-field crontab format, and both schedulers accept any 5-field
crontab expression. From 4.8.0 the Quartz scheduler converts it to Quartz's format (a seconds field first, '?' in the
unused day field, days of week renumbered from Sunday = 1), so `0 0 * * 1` still means Monday. A crontab expression
that restricts both the day of month and the day of week cannot be expressed in Quartz and throws
`ArgumentException`, as does `L` or `#` in the day of week. A 6- or 7-field expression is passed to Quartz unchanged (for example `0 0 6 ? * MON-FRI`).

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
