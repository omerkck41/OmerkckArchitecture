# Kck.Templates

`dotnet new` templates for Kck-based .NET projects.

## Install

```bash
dotnet new install Kck.Templates
```

## Templates

### `kck-webapi` — Kck WebAPI

Scaffolds a WebAPI project pre-wired with `Kck.Bundle.WebApi`:

| Feature | Default Provider |
|---|---|
| Logging | Serilog (structured JSON) |
| Caching | In-memory (swap for Redis/Hybrid) |
| Security | JWT RS256 + Argon2 hashing |
| Pipeline | Mediator 3.x behaviors |
| Event Bus | In-memory (swap for RabbitMQ/Azure) |
| Observability | OpenTelemetry (traces + metrics) |
| ASP.NET Core | Rate limiting + security headers + CORS + exception handling |

### Usage

```bash
# Create with sample WeatherForecast feature (default)
dotnet new kck-webapi -n MyApp

# Create without the sample feature
dotnet new kck-webapi -n MyApp --IncludeSample false

# List all options
dotnet new kck-webapi --help
```

## Uninstall

```bash
dotnet new uninstall Kck.Templates
```
