# Kck.Observability.OpenTelemetry

OpenTelemetry-based observability provider for Kck applications — wires distributed tracing, metrics, health checks, and OTLP export in one call.

## Installation

```bash
dotnet add package Kck.Observability.OpenTelemetry
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckObservability(obs =>
{
    obs.ServiceName = "order-service";
    obs.OtlpEndpoint = new Uri("http://otel-collector:4317");

    obs.UseHealthChecks(hc =>
    {
        hc.AddDbContextCheck<AppDbContext>();
        hc.AddRedis("localhost:6379");
    });
});

var app = builder.Build();
app.MapHealthChecks("/health");
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ServiceName` | OTEL service name | assembly name |
| `OtlpEndpoint` | OTLP gRPC/HTTP collector endpoint | `null` (console exporter) |
| `EnableTracing` | Register distributed tracing | `true` |
| `EnableMetrics` | Register runtime and HTTP metrics | `true` |
| `HealthCheckPath` | Health endpoint route | `"/health"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/observability.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
