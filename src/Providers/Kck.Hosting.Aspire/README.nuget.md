# Kck.Hosting.Aspire

.NET Aspire integration for Kck applications: `AddKckServiceDefaults()` adds Aspire service discovery (also for every
`HttpClient`) and the health check services in a single call. Add OpenTelemetry with `Kck.Observability.OpenTelemetry`.

## Installation

```bash
dotnet add package Kck.Hosting.Aspire
```

## Quick Start

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Adds Aspire service discovery (also for every HttpClient) and the health check services
builder.AddKckServiceDefaults();

var app = builder.Build();
app.MapHealthChecks("/health");
app.Run();
```

`AddKckServiceDefaults()` has no options. Map the health endpoint yourself (`app.MapHealthChecks(...)`, or
`MapKckHealthChecks()` from Kck.Observability.OpenTelemetry for liveness/readiness probes).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/observability.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
