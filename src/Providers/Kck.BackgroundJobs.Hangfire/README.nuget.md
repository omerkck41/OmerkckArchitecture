# Kck.BackgroundJobs.Hangfire

Hangfire-backed `IJobScheduler` implementation for reliable background job processing with persistent storage and a built-in dashboard.

## Installation

```bash
dotnet add package Kck.BackgroundJobs.Hangfire
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckHangfire(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("Hangfire")!;
    options.DashboardPath = "/jobs";
});

var app = builder.Build();
app.UseKckHangfireDashboard();

// Schedule a job
app.Services.GetRequiredService<IJobScheduler>()
   .EnqueueAsync<IEmailService>(s => s.SendAsync("hello@example.com"));
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ConnectionString` | SQL Server / PostgreSQL connection string | required |
| `DashboardPath` | Hangfire dashboard route | `"/hangfire"` |
| `WorkerCount` | Number of background worker threads | `5` |
| `RetryAttempts` | Automatic retry count on failure | `3` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/background-jobs.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
