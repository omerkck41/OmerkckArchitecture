# Kck.Bundle.WebApi

One-line ASP.NET Core Web API setup: registers Serilog, Argon2 hashing, the in-memory cache, the in-memory event bus, exception handling, health check services, and rate limiting with security headers and CORS from the `Kck` configuration section. With `Kck:Security:Jwt:Issuer` set it also registers the JWT token service; ASP.NET Core authentication, persistence, the Mediator pipeline and OpenTelemetry are configured separately.

New to Kck? [Your first Kck API in five minutes](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/getting-started/README.md) creates a working API with this package: `dotnet new install Kck.Templates`, then `dotnet new kck-webapi`.

## Installation

```bash
dotnet add package Kck.Bundle.WebApi
```

## Quick Start

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKckWebApiDefaults(builder.Configuration);

var app = builder.Build();

app.UseKckWebApiDefaults();

app.MapControllers();

app.Run();
```

That is all the setup required. Add your controllers, Mediator handlers, and domain entities — the bundle wires everything else.

## Configuration

| Property | Description | Default |
|---|---|---|
| `Kck:Security:Jwt:Issuer` | JWT token issuer; setting it registers the JWT token service | — |
| `Kck:Security:Jwt:Audience` | JWT token audience; required once `Issuer` is set | — |
| `Kck:AspNetCore:RateLimitPermitLimit` | Requests per window per client IP | `100` |
| `Kck:AspNetCore:RateLimitWindowSeconds` | Rate limit window (seconds) | `60` |
| `Kck:AspNetCore:SecurityHeaders` | Adds the security headers middleware | `true` |
| `Kck:AspNetCore:CorsOrigins` | Allowed CORS origins (string array, for example `["https://myapp.com"]`) | none — CORS off |

These are the only settings the bundle reads. The cache is always the in-memory one and the health checks have no
configuration; configure the providers in code for anything else.

`Issuer` alone is not enough: the JWT options are validated at start-up, and without `Audience` and an RS256 signing
key start-up fails with a `JwtOptions is invalid` error that lists `Audience: null or empty` and
`RsaKeyBase64 … is empty`. The bundle does not bind the key; set it (and any
other `JwtOptions` value, such as `AccessTokenExpiration`) on `JwtOptions` yourself:

```csharp
using Kck.Security.Jwt;

builder.Services.AddKckWebApiDefaults(builder.Configuration);
builder.Services.Configure<JwtOptions>(jwt =>
    jwt.RsaKeyBase64 = Environment.GetEnvironmentVariable("JWT_RSA_KEY_B64"));
```

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/README.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
