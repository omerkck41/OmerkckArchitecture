# Kck.Bundle.WorkerService

Worker Service setup in a single call: Serilog, health check services, the InMemory event bus and the Quartz.NET
background job scheduler. OpenTelemetry is configured separately (Kck.Observability.OpenTelemetry).

## Installation

```bash
dotnet add package Kck.Bundle.WorkerService
```

## Quick Start

```csharp
// Program.cs
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddKckWorkerServiceDefaults(builder.Configuration);
builder.Services.AddKckJob<DailyReportJob>();   // jobs are resolved from DI in a new scope per execution

var host = builder.Build();
await host.Services.GetRequiredService<IJobScheduler>()
    .AddOrUpdateRecurringAsync<DailyReportJob>("daily-report", "0 0 8 * * ?"); // every day at 08:00
await host.RunAsync();

// Define a job
public sealed class DailyReportJob(IReportService reports) : IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default) => reports.GenerateDailyAsync(ct);
}
```

## Configuration (`Kck` section)

| Property | Description | Default |
|---|---|---|
| `BackgroundJobs:Provider` | Background job scheduler. Only `Quartz` is bundled. | `Quartz` |
| `Observability:Tracing` / `Metrics` | Enable tracing / metrics | `true` |
| `Observability:OtlpEndpoint` | OTLP exporter endpoint | — |
| `Observability:ServiceName` | Service name reported to OpenTelemetry | — |

## Hangfire

Since 4.0 the bundle does not include Hangfire (LGPL-3.0). Setting `BackgroundJobs:Provider` to `Hangfire` fails at
startup with instructions instead of silently switching schedulers. To keep Hangfire, reference
`Kck.BackgroundJobs.Hangfire` and register it yourself with `AddKckBackgroundJobsHangfire(...)`.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/README.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
