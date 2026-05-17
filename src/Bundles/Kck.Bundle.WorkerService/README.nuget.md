# Kck.Bundle.WorkerService

Worker Service setup with background jobs, Serilog, and OpenTelemetry — registers the InMemory event bus and a mutually-exclusive background job scheduler (Hangfire or Quartz) in a single call.

## Installation

```bash
dotnet add package Kck.Bundle.WorkerService
```

## Quick Start

```csharp
// Program.cs
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddKckWorkerServiceDefaults(builder.Configuration);

// Register your background job implementations
builder.Services.AddScoped<DailyReportJob>();

var host = builder.Build();
host.Run();

// Define a job
public class DailyReportJob(IReportService reports, IEmailService email) : IJob
{
    public async Task ExecuteAsync(JobContext context, CancellationToken ct)
    {
        var report = await reports.GenerateDailyAsync(ct);
        await email.SendAsync(new EmailMessage
        {
            To      = [new EmailRecipient("ops@company.com")],
            Subject = "Daily Report",
            Body    = report.HtmlContent,
            IsHtml  = true
        }, ct);
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `BackgroundJobs:Provider` | `Hangfire` or `Quartz` | `Hangfire` |
| `BackgroundJobs:ConnectionString` | Storage connection string | In-memory |
| `BackgroundJobs:WorkerCount` | Concurrent job worker count | `5` |
| `Observability:Otlp:Endpoint` | OTLP exporter endpoint | `"http://localhost:4317"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/README.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
