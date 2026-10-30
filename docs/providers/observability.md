# Observability

OpenTelemetry integration for tracing, metrics and health checks.
It collects RED (Rate, Errors, Duration) and USE (Utilization, Saturation, Errors)
signals automatically.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Observability.Abstractions` | `IMetricsService`, `ITracingService`, health contracts |
| `Kck.Observability.OpenTelemetry` | OTel auto-instrumentation + custom meter |

## Setup

```csharp
services.AddKckObservabilityOpenTelemetry(opt =>
{
    opt.ServiceName = "orders-api";
    opt.ServiceVersion = "1.2.0";
    opt.OtlpEndpoint = "http://otel-collector:4317";
    opt.EnableConsoleExporter = false;
    opt.EnableAspNetCore = true;
    opt.EnableHttpClient = true;
    opt.EnableEntityFramework = true;
});
```

## Auto-Instrumentation

Each instrumentation is enabled through configuration:

| Flag | Instrumentation |
|---|---|
| `EnableAspNetCore` | HTTP request trace + `http.server.duration` metric |
| `EnableHttpClient` | HttpClient request trace + egress metric |
| `EnableEntityFramework` | EF Core command trace + `db.client.duration` |

## Custom Metrics

```csharp
public class OrderService(IMetricsService metrics)
{
    public async Task ProcessAsync(Order order, CancellationToken ct)
    {
        metrics.Increment("orders.created", 1, new() { ["tenant"] = order.TenantId });
        using var timer = metrics.StartTimer("order.process.duration");
        // ...
    }
}
```

## Custom Tracing

Auto-instrumentation does not capture business logic with p99 > 200 ms. Create a manual span:

```csharp
public class PaymentService(ITracingService tracing)
{
    public async Task ChargeAsync(PaymentRequest req, CancellationToken ct)
    {
        using var span = tracing.StartSpan("payment.charge");
        span.SetAttribute("payment.provider", "stripe");
        span.SetAttribute("payment.amount", req.Amount);
        // ...
    }
}
```

## Health Checks

```csharp
services.AddKckHealthChecks()
    .AddKckRedisHealthCheck()
    .AddKckDbContextHealthCheck<MyDbContext>();

app.MapKckHealthEndpoints("/health");
```

Endpoints:

| Path | Purpose |
|---|---|
| `/health/live` | Is the process alive? (liveness probe) |
| `/health/ready` | Can it accept requests? (readiness probe: DB, cache, dependencies) |
| `/health/startup` | Has startup finished? (startup probe) |

## Logging Correlation

The Serilog `TraceContextEnricher` adds `TraceId` and `SpanId` to every log line,
which makes it easier to correlate logs with traces
([Logging guide](logging.md#trace-correlation)).

## Sampling

Default in production: **parent-based + head sampling (10%)**. To raise the
sampling rate (for debugging), use `Configure()`.

## PII Safety

- Do not write PII to span attributes. Use masked or hashed values.
- The `db.statement` attribute contains raw SQL; ORM parameters are kept separate.
- Logged headers pass through a default blocklist (`Authorization`,
  `Cookie`, etc.).
