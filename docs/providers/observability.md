# Observability

OpenTelemetry integration for tracing and metrics, a small metrics/tracing abstraction for your own signals, and
liveness/readiness/startup health checks.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Observability.Abstractions` | `IMetricsService`, `ITracingService`, health contracts |
| `Kck.Observability.OpenTelemetry` | OTel auto-instrumentation + custom meter |

## Setup

```csharp
using Kck.Observability.Abstractions;

services.AddKckObservability(obs => obs
    .UseOpenTelemetry(otel =>
    {
        otel.ServiceName = "orders-api";
        otel.EnableTracing()
            .EnableMetrics()
            .UseOtlpExporter("http://otel-collector:4317");
    })
    .UseHealthChecks(health => health
        .AddReadinessCheck("orders-db", async ct => new HealthCheckResult(HealthStatus.Healthy))));

// ...

app.MapKckHealthChecks(); // /health, /health/live, /health/ready, /health/startup
```

`UseOpenTelemetry` registers `ITracingService` and `IMetricsService` for the service name. Without
`UseOtlpExporter` nothing is exported.

## Auto-Instrumentation

| Enabled by | Instrumentation |
|---|---|
| `EnableTracing()` | ASP.NET Core requests, `HttpClient` calls, EF Core commands, and the `ServiceName` and `Kck.Application` activity sources |
| `EnableMetrics()` | ASP.NET Core request metrics and the `ServiceName` and `Kck.Application` meters |

## Custom Metrics

```csharp
using Kck.Observability.Abstractions;

public class OrderMetrics(IMetricsService metrics)
{
    private readonly ICounter _created = metrics.CreateCounter("orders.created", "Orders created");
    private readonly IHistogram _duration = metrics.CreateHistogram("orders.process.duration", "Processing time (ms)");

    public void Created(Order order, double elapsedMs)
    {
        _created.Increment(1, new KeyValuePair<string, object?>("tenant", order.TenantId));
        _duration.Record(elapsedMs);
    }
}
```

Create instruments once (for example in a singleton) and reuse them.

## Custom Tracing

```csharp
using Kck.Observability.Abstractions;

public class PaymentService(ITracingService tracing)
{
    public async Task ChargeAsync(PaymentRequest request, CancellationToken ct)
    {
        using var span = tracing.StartSpan("payment.charge", SpanKind.Client);
        span.SetAttribute("payment.provider", "stripe");
        try
        {
            await Task.Delay(10, ct); // call the payment provider
            span.SetStatus(SpanStatus.Ok);
        }
        catch (Exception ex)
        {
            span.RecordException(ex);
            span.SetStatus(SpanStatus.Error, ex.Message);
            throw;
        }
    }
}
```

## Health Checks

`UseHealthChecks` adds checks to ASP.NET Core health checks with a tag per probe: `AddLivenessCheck`,
`AddReadinessCheck`, `AddStartupCheck`, or `AddCheck` (untagged). `MapKckHealthChecks(basePath = "/health")` maps:

| Path | Runs |
|---|---|
| `/health/live` | liveness checks and untagged checks |
| `/health/ready` | readiness checks and untagged checks (database, cache, dependencies) |
| `/health/startup` | startup checks and untagged checks |
| `/health` | all checks |

Providers add their own checks to the same system (for example `rabbitmq-eventbus` with the `ready` tag).

## Logging Correlation

`Kck.Logging.Serilog` adds `TraceId` and `SpanId` to every log event (on by default, `WithTraceCorrelation()`), which
correlates logs with traces ([Logging guide](logging.md#trace-correlation)).

## Sampling

No sampler is configured, so the OpenTelemetry SDK default applies (parent-based, always on). To sample, configure
the SDK with `services.ConfigureOpenTelemetryTracerProvider(...)`.

## PII Safety

- Do not write PII to span attributes or metric tags; use masked or hashed values.
- High-cardinality tags (user ID, order ID) make metric storage expensive; keep them on spans.
