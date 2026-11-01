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
| `Kck:Security:Jwt:Issuer` | JWT token issuer; setting it registers the JWT token service | — |
| `Kck:Security:Jwt:Audience` | JWT token audience; required once `Issuer` is set | — |

These are the only settings the bundle reads. The cache is always the in-memory one and the health checks have no
configuration; configure the providers in code for anything else.

`Issuer` alone is not enough: the JWT options are validated at start-up, and without `Audience` and an RS256 signing
key the application fails to start with `JwtOptions is invalid: • Audience: null or empty` and
`• RsaKeyBase64: KeySource=Configuration but RsaKeyBase64 is empty`. The bundle does not bind the key; set it (and any
other `JwtOptions` value, such as `AccessTokenExpiration`) on `JwtOptions` yourself:

```csharp
using Kck.Security.Jwt;

builder.Services.AddKckMinimalApiDefaults(builder.Configuration);
builder.Services.Configure<JwtOptions>(jwt =>
    jwt.RsaKeyBase64 = Environment.GetEnvironmentVariable("JWT_RSA_KEY_B64"));
```

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/README.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
