# Kck.Logging.Serilog

Serilog integration for Kck applications with structured request logging, enrichers, and sink configuration via `appsettings.json`.

## Installation

```bash
dotnet add package Kck.Logging.Serilog
```

## Quick Start

```csharp
// Program.cs
builder.AddKckSerilog();

var app = builder.Build();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (ctx, httpContext) =>
    {
        ctx.Set("UserId", httpContext.User?.Identity?.Name ?? "anonymous");
    };
});
```

```json
// appsettings.json
{
  "Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/app-.log", "rollingInterval": "Day" } }
    ]
  }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `MinimumLevel` | Global minimum log level | `Information` |
| `WriteTo` | Serilog sink configuration array | Console sink |
| `Enrich` | Enricher list (`FromLogContext`, `WithMachineName`) | `FromLogContext` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/logging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
