# Background Jobs

Arka plan is planlamasi icin provider-agnostic abstraction. `IJobScheduler`
arayuzu asagidaki teknolojilerle implement edilir.

## Paketler

| Paket | Hedef |
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

Dashboard icin:

```csharp
app.UseHangfireDashboard("/jobs");
```

> Not: `Hangfire.MySqlStorage` kaldirildi — terkedilmis paketti
> ([ADR-0002](../adr/0002-hangfire-storage.md)). SQL Server, PostgreSQL veya
> InMemory desteklenir.

## Quartz

```csharp
services.AddKckBackgroundJobsQuartz();                         // varsayilanlar: in-memory store
services.AddKckBackgroundJobsQuartz(q => q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10)); // Quartz.NET 4 IQuartzBuilder
```

Quartz hosted service otomatik eklenir (`WaitForJobsToComplete = true`); isler DI'dan cozulur. 4.0 itibariyla Quartz.NET
4 kullanilir — Quartz 3'ten gecis notlari: [v3-to-v4](../migrations/v3-to-v4.md#7-quartznet-4).

> `Kck.Bundle.WorkerService` 4.0'dan itibaren yalniz Quartz icerir; Hangfire (LGPL-3.0) icin
> `Kck.BackgroundJobs.Hangfire` paketini ayrica ekleyin.

## Job Yazma

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

DI kaydi icin [ADR-0007](../adr/0007-add-kck-job-helper.md) helper:

```csharp
services.AddKckJob<EmailCleanupJob>();
```

## Scheduler Kullanimi

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

## Cron Sabitleri

`CronExpressions` hazir sabitler sunar:

| Sabit | Ifade |
|---|---|
| `EveryMinute` | `* * * * *` |
| `EveryHour` | `0 * * * *` |
| `Every5Minutes` | `*/5 * * * *` |
| `EveryDay` | `0 0 * * *` |
| `EveryWeek` | `0 0 * * 1` |
| `EveryMonth` | `0 0 1 * *` |

## Secim Kriterleri

| Kriter | Hangfire | Quartz |
|---|---|---|
| Storage | SQL Server / PostgreSQL / Memory | In-process (persistence opsiyonel) |
| Dashboard | Yerlesik | Uc-parti |
| Distributed | Evet (worker pool) | Evet (clustering) |
| Cron | Cron + TimeSpan | Full Quartz cron |
| Best for | Web app job queue | Scheduler-heavy workload |
