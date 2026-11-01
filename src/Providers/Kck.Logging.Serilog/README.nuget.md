# Kck.Logging.Serilog

Serilog integration for Kck applications: console, compact JSON and rolling file output, enrichers and PII masking.
Works in web, worker and console hosts; it does not depend on the ASP.NET Core shared framework.

## Installation

```bash
dotnet add package Kck.Logging.Serilog
```

## Quick Start

```csharp
using Serilog;

// Program.cs
builder.Services.AddKckSerilog(log => log
    .WithApplicationName("OrderService")
    .WriteToCompactJson()        // JSON to the console; WriteToConsole() for text (the default)
    .UsePiiMasking());           // masks password, token, email... properties
```

HTTP request logging (`app.UseSerilogRequestLogging()`) lives in `Serilog.AspNetCore`; since 5.0 this package no
longer brings it, so a web application that wants it adds `Serilog.AspNetCore` itself.

## Configuration

`AddKckSerilog` configures Serilog in code; it does not read `appsettings.json`. It sets the minimum level to
`Information`, enriches every event from the log context, with an `Application` property (`WithApplicationName`,
default `KckApp`) and with `TraceId` / `SpanId` (`WithTraceCorrelation`, on by default), and writes to the console as
text (`WriteToConsole`, the default) or compact JSON (`WriteToCompactJson`), optionally to a rolling file
(`WriteToFile(path, interval)`, 31 files kept). `UsePiiMasking` / `MaskPiiProperties` replace sensitive property
values with `***`.

`Configure(Action<LoggerConfiguration>)` gives raw access to the logger configuration; it runs after the settings
above. For configuration-driven sinks and levels, add the `Serilog.Settings.Configuration` package and call
`ReadFrom.Configuration` there, for example
`builder.Services.AddKckSerilog(log => log.Configure(c => c.ReadFrom.Configuration(builder.Configuration)))`.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/logging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
