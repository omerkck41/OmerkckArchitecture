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
`UseOtlpExporter` nothing is exported. The reference app exports only when `OTEL_EXPORTER_OTLP_ENDPOINT` is set:
[`Program.cs`](https://github.com/omerkck41/OmerkckArchitecture/blob/main/samples/Kck.Sample.Orders/Program.cs).

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
    private readonly IHistogram _duration = metrics.CreateHistogram(
        "orders.process.duration", "Processing time (ms)", buckets: [5, 10, 25, 50, 100, 250, 500, 1000]);

    public void Created(Order order, double elapsedMs)
    {
        _created.Increment(1, new KeyValuePair<string, object?>("tenant", order.TenantId));
        _duration.Record(elapsedMs);
    }
}
```

- `CreateCounter`, `CreateHistogram` and `CreateGauge` create an instrument on the first call for a name and return the
  same instance for every later call with that name (case-insensitive); the description and buckets of the first call
  win. Keeping the instrument in a field, as above, avoids the lookup.
- `buckets` are the histogram's explicit bucket boundaries, published as OpenTelemetry instrument advice
  (`InstrumentAdvice<double>.HistogramBucketBoundaries`); an exporter view configured for the instrument overrides them.
  Without `buckets` the SDK's default boundaries apply. The boundaries must be strictly increasing (no unsorted or
  duplicate values) and finite (no NaN or infinity); otherwise `CreateHistogram` throws `ArgumentException` for
  `buckets`, nothing is cached, and a later call with valid buckets creates the instrument. Before 5.0 the argument was
  ignored, so invalid buckets never failed.
- Tags go on each measurement (`Increment(value, tags)`, `Record(value, tags)`, `Set(value, tags)`). The `tags`
  parameter of `CreateCounter` (a list of tag keys without values, which OpenTelemetry cannot represent and the provider
  ignored) was removed in 5.0.

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
