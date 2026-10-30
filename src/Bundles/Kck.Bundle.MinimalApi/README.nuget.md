# Kck.Bundle.MinimalApi

Lightweight Minimal API setup, a slim variant of `Kck.Bundle.WebApi`: Serilog, the in-memory cache, exception handling
and health check services, plus the JWT token service when `Kck:Security:Jwt:Issuer` is set. No Argon2, event bus or
rate limiting; ASP.NET Core authentication is configured separately.

## Installation

```bash
dotnet add package Kck.Bundle.MinimalApi
```

## Quick Start

```csharp
// Program.cs
using Kck.Exceptions.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKckMinimalApiDefaults(builder.Configuration);

var app = builder.Build();

app.UseKckExceptionHandling();   // RFC 7807 problem details for unhandled exceptions

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
