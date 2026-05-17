# Kck.Bundle.MinimalApi

Lightweight Minimal API setup — slim variant of `Kck.Bundle.WebApi` providing Serilog, JWT auth, in-memory caching, exception handling, and health checks without MediatR pipeline or Argon2 overhead.

## Installation

```bash
dotnet add package Kck.Bundle.MinimalApi
```

## Quick Start

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKckMinimalApiDefaults(builder.Configuration);

var app = builder.Build();

app.UseKckMinimalApiDefaults();

// Map your minimal API endpoints
app.MapGet("/products/{id:int}", async (int id, IProductService svc, CancellationToken ct)
    => await svc.GetAsync(id, ct) is { } product
        ? Results.Ok(product)
        : Results.NotFound());

app.MapHealthChecks("/health");

app.Run();
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Security:Jwt:Issuer` | JWT token issuer | — |
| `Security:Jwt:Audience` | JWT token audience | — |
| `Security:Jwt:ExpiryMinutes` | Access token lifetime (minutes) | `60` |
| `Caching:Provider` | Cache backend (`InMemory`, `Redis`) | `InMemory` |
| `Observability:Otlp:Endpoint` | OTLP exporter endpoint | `"http://localhost:4317"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/README.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
