# Logging

Serilog-based structured logging. Supports trace correlation, JSON output
and rolling files.

## Packages

| Package | Target |
|---|---|
| `Kck.Logging.Serilog` | `AddKckSerilog()` + `KckSerilogBuilder` |

## Setup

```csharp
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
| `WriteToConsole()` | Standard console sink (rendered text) |
| `WriteToCompactJson()` | `RenderedCompactJsonFormatter`, compatible with log aggregators |
| `WriteToFile(path, interval)` | Rolling file sink (default: Day, 31 files) |
| `WithTraceCorrelation(enabled)` | `TraceId`, `SpanId`, `ParentSpanId` properties |
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
logger.LogInformation("User {UserId} placed order {OrderId} for {Amount}",
    userId, orderId, amount);
```

Forbidden (string interpolation):

```csharp
// WRONG — the logger does not extract parameters
logger.LogInformation($"User {userId} placed order {orderId}");
```

## PII / Sensitive Data

- Never log passwords, tokens or CVV. Add a filter in middleware.
- For a logger created from `ILoggerFactory`, you can use the `ForContext<T>()`
  scope pattern.
- GDPR: log PII hashed or masked (E.164 → mask all but the last 4 digits).

## Log Retention

`WriteToFile` defaults to `retainedFileCountLimit: 31` (31 days). To increase
it, add `.WriteTo.File(..., retainedFileCountLimit: 90)` through `Configure()`.
