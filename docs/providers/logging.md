# Logging

Serilog-based structured logging. Supports trace correlation, JSON output
and rolling files.

## Packages

| Package | Target |
|---|---|
| `Kck.Logging.Serilog` | `AddKckSerilog()` + `KckSerilogBuilder` |

Since 5.0 the package depends on `Serilog.Extensions.Hosting`, `Serilog.Formatting.Compact`, `Serilog.Sinks.Console`
and `Serilog.Sinks.File` instead of `Serilog.AspNetCore`, so it needs no ASP.NET Core shared framework and works in
worker and console hosts (ADR-0032). A web application that wants HTTP request logging adds `Serilog.AspNetCore` and
calls `app.UseSerilogRequestLogging()`; `WriteTo.Debug()` needs `Serilog.Sinks.Debug`.

`AddKckSerilog` configures Serilog in code and does not read `appsettings.json`. For configuration-driven sinks and
levels, add `Serilog.Settings.Configuration` and call it through the builder's `Configure` hook:
`log.Configure(c => c.ReadFrom.Configuration(builder.Configuration))`.

## Setup

```csharp
using Serilog;

builder.Services.AddKckSerilog(log => log
    .WithApplicationName("OrderService")
    .WriteToCompactJson()
    .WriteToFile("logs/app-.log", RollingInterval.Day)
    .WithTraceCorrelation());
```

## Builder API

| Method | Description |
|---|---|
| `WithApplicationName(name)` | Adds the `Application` property to every log |
| `WriteToConsole()` | Console sink with rendered text (the default) |
| `WriteToCompactJson()` | Console sink with `RenderedCompactJsonFormatter`, for log aggregators (replaces the text console output) |
| `WriteToFile(path, interval)` | Rolling file sink (default: Day; `RetainedFileCount` = 31 files) |
| `WithTraceCorrelation(enabled)` | `TraceId`, `SpanId`, `ParentSpanId` properties (on by default) |
| `UsePiiMasking()` / `MaskPiiProperties(names)` | Replaces the values of sensitive properties with `***` |
| `Configure(action)` | Raw `LoggerConfiguration` access |

## Trace Correlation

`WithTraceCorrelation(true)` is the default. When `Activity.Current` exists,
`TraceId` and `SpanId` are added to every log event. This correlates logs and
traces with OpenTelemetry.

## Log Levels

The default minimum level is `Information`. To raise verbosity in development:

```csharp
builder.Services.AddKckSerilog(log => log
    .Configure(c => c.MinimumLevel.Debug()));
```

## Structured Log Examples

Correct usage (template):

```csharp
public sealed class CheckoutLog(ILogger<CheckoutLog> logger)
{
    public void OrderPlaced(Guid userId, Guid orderId, decimal amount) =>
        logger.LogInformation("User {UserId} placed order {OrderId} for {Amount}", userId, orderId, amount);
}
```

Forbidden (string interpolation):

```csharp
public sealed class CheckoutLogWrong(ILogger<CheckoutLogWrong> logger)
{
    // WRONG — the message is formatted before logging: no UserId/OrderId properties, and a new template per call
    public void OrderPlaced(Guid userId, Guid orderId) =>
        logger.LogInformation($"User {userId} placed order {orderId}");
}
```

## PII / Sensitive Data

- Never log passwords, tokens or CVV.
- `UsePiiMasking()` masks structured properties named like `password`, `token`, `apikey`, `email`, `phone`,
  `creditcard` or `cvv` (case-insensitive); add your own names with `MaskPiiProperties(...)`. It masks property
  values, not text inside a message, so keep sensitive data in template properties.
- GDPR: log PII hashed or masked.

## Log Retention

`WriteToFile` keeps 31 files by default (31 days with the daily interval). Set `RetainedFileCount` to change it:

```csharp
builder.Services.AddKckSerilog(log =>
{
    log.WriteToFile("logs/app-.log");
    log.RetainedFileCount = 90;
});
```
