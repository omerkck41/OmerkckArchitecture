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
| `Security:Jwt:Issuer` | JWT token issuer | — |
| `Security:Jwt:Audience` | JWT token audience | — |
| `Security:Jwt:ExpiryMinutes` | Access token lifetime (minutes) | `60` |
| `Caching:Provider` | Cache backend (`InMemory`, `Redis`) | `InMemory` |
| `AspNetCore:Cors:AllowedOrigins` | Comma-separated allowed CORS origins | `"*"` |
| `AspNetCore:RateLimitPermitLimit` | Requests per window per client | `100` |
| `Observability:Otlp:Endpoint` | OTLP exporter endpoint | `"http://localhost:4317"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/README.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
