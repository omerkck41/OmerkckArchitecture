# Kck.Observability.OpenTelemetry

OpenTelemetry-based observability provider for Kck applications — wires distributed tracing, metrics, health checks, and OTLP export in one call.

## Installation

```bash
dotnet add package Kck.Observability.OpenTelemetry
```

## Quick Start

```csharp
using Kck.Observability.Abstractions;

// Program.cs
builder.Services.AddKckObservability(obs => obs
    .UseOpenTelemetry(otel =>
    {
        otel.ServiceName = "order-service";
        otel.EnableTracing()
            .EnableMetrics()
            .UseOtlpExporter("http://otel-collector:4317");
    })
    .UseHealthChecks(health => health
        .AddReadinessCheck("database", async ct => new HealthCheckResult(HealthStatus.Healthy))));

var app = builder.Build();
app.MapKckHealthChecks();   // /health, /health/live, /health/ready, /health/startup
```

## Configuration

| Builder member | Effect |
|---|---|
| `UseOpenTelemetry(o => ...)`: `ServiceName` | Resource service name (default `"KckApp"`) |
| `EnableTracing()` | ASP.NET Core, `HttpClient` and EF Core tracing, `Kck.Application` source |
| `EnableMetrics()` | ASP.NET Core metrics and the `Kck.Application` meter |
| `UseOtlpExporter(endpoint)` | OTLP export; without it nothing is exported |
| `UseHealthChecks(h => ...)` | `AddLivenessCheck` / `AddReadinessCheck` / `AddStartupCheck` / `AddCheck` |
| `app.MapKckHealthChecks(basePath)` | `/health`, `/health/live`, `/health/ready`, `/health/startup` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/observability.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
