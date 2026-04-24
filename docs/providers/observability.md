# Observability

Tracing, metrics ve health check icin OpenTelemetry entegrasyonu.
RED (Rate, Errors, Duration) + USE (Utilization, Saturation, Errors)
sinyallerini otomatik toplar.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Observability.Abstractions` | `IMetricsService`, `ITracingService`, health kontratları |
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

Konfigurasyon bazli olarak ekli:

| Flag | Instrumentation |
|---|---|
| `EnableAspNetCore` | HTTP istek trace + `http.server.duration` metric |
| `EnableHttpClient` | HttpClient istek trace + egress metric |
| `EnableEntityFramework` | EF Core komut trace + `db.client.duration` |

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

Auto-instrumentation p99 > 200ms olan business logic'i yakalamaz — manuel span:

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

Endpoint'ler:

| Path | Amac |
|---|---|
| `/health/live` | Process ayakta mi? (liveness probe) |
| `/health/ready` | Istek alabilir mi? (readiness probe: DB, cache, dependency) |
| `/health/startup` | Baslatma tamamlandi mi? (startup probe) |

## Logging Correlation

Serilog `TraceContextEnricher` ile her log satirina `TraceId` + `SpanId`
eklenir — logları trace ile korelasyon kolaylasır
([Logging rehberi](logging.md#trace-correlation)).

## Sampling

Production'da varsayilan: **parent-based + head sampling (%10)**. Ornekleme
oranini artirmak icin (debug) `Configure()` uzerinden.

## PII Guvenligi

- Span attribute'larina PII yazma — masked / hashed degerler kullan
- `db.statement` attribute'una raw SQL yazılır; ORM parametreler ayri tutulur
- Log'lanan header'lar default blocklist'ten gecirilir (`Authorization`,
  `Cookie` vs.)
