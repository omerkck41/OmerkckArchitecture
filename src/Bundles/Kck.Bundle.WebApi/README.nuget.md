# Kck.Bundle.WebApi

One-line ASP.NET Core WebAPI setup — registers Serilog, JWT auth, Argon2 hashing, in-memory caching, MediatR pipeline, InMemory event bus, OpenTelemetry, rate limiting, security headers, CORS, and global exception handling in a single call.

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

That is all the setup required. Add your controllers, MediatR handlers, and domain entities — the bundle wires everything else.

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
