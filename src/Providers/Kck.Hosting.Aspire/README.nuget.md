# Kck.Hosting.Aspire

.NET Aspire service defaults integration for Kck applications — wires OpenTelemetry, health checks, and service discovery in a single call.

## Installation

```bash
dotnet add package Kck.Hosting.Aspire
```

## Quick Start

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Registers OpenTelemetry tracing/metrics, health checks, and Aspire service discovery
builder.AddKckServiceDefaults();

var app = builder.Build();
app.MapDefaultEndpoints(); // /health, /alive
app.Run();
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ServiceName` | OTEL service name override | assembly name |
| `EnableMetrics` | Register OpenTelemetry metrics | `true` |
| `EnableTracing` | Register OpenTelemetry distributed tracing | `true` |
| `HealthCheckPath` | Health check endpoint path | `"/health"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/observability.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
