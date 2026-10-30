# Kck Framework — Configuration Guide

## Providers are configured in code

Provider packages do not read `appsettings.json` on their own. Each registration takes an options callback, and you
decide where the values come from:

```csharp
services.AddKckCachingRedis(
    redis => redis.Configuration = builder.Configuration["Redis:ConnectionString"],
    cache => cache.KeyPrefix = "myapp:");

services.AddKckArgon2(opt => builder.Configuration.GetSection("Argon2").Bind(opt));
```

The options and their defaults are listed in each [provider guide](providers/README.md). Keep secrets (connection
strings, API keys, signing keys) in user secrets, environment variables or a secret store, not in `appsettings.json`.

## The `Kck` section (bundles only)

The bundles read a `"Kck"` section. For IDE autocomplete, add the schema to your `appsettings.json`:

```json
{
  "$schema": "https://raw.githubusercontent.com/omerkck41/OmerkckArchitecture/main/schemas/appsettings.kck.schema.json"
}
```

| Key | Read by | Default | Effect |
|---|---|---|---|
| `Kck:Security:Jwt:Issuer` | WebApi, MinimalApi | — | When set, registers the JWT token service (`Kck.Security.Jwt`) with this issuer |
| `Kck:Security:Jwt:Audience` | WebApi, MinimalApi | `""` | Audience of the token service |
| `Kck:AspNetCore:RateLimitPermitLimit` | WebApi | `100` | Requests per client IP per window |
| `Kck:AspNetCore:RateLimitWindowSeconds` | WebApi | `60` | Rate-limit window in seconds |
| `Kck:AspNetCore:SecurityHeaders` | WebApi | `true` | Security response headers |
| `Kck:AspNetCore:CorsOrigins` | WebApi | — | Allowed CORS origins; CORS is off when left out |
| `Kck:BackgroundJobs:Provider` | WorkerService | `Quartz` | Only `Quartz` is bundled; any other value fails at startup |

```json
{
  "$schema": "https://raw.githubusercontent.com/omerkck41/OmerkckArchitecture/main/schemas/appsettings.kck.schema.json",
  "Kck": {
    "Security": {
      "Jwt": {
        "Issuer": "https://auth.example.com",
        "Audience": "https://api.example.com"
      }
    },
    "AspNetCore": {
      "RateLimitPermitLimit": 100,
      "RateLimitWindowSeconds": 60,
      "SecurityHeaders": true,
      "CorsOrigins": ["https://app.example.com"]
    }
  }
}
```

The JWT token service still needs its RSA signing key, which the bundles do not read from configuration. Configure it
in code with `services.Configure<JwtOptions>(...)` or call `AddKckJwt(...)` yourself (see [Security](providers/security.md#jwt)).

Not read in this version: the bundles' default classes also declare `Persistence`, `Caching`, `Observability` and
`Security:Jwt:Algorithm` / `Security:Hashing` properties, but the bundles do not use them; configure those providers
in code.
