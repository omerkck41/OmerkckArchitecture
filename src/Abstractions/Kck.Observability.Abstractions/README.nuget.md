# Kck.Observability.Abstractions

Observability abstractions — distributed tracing (`ITracingService`), structured metrics (`IMetricsService`), and health checks (`IHealthCheck`) backed by the OpenTelemetry provider.

## Installation

```bash
dotnet add package Kck.Observability.Abstractions
```

## Quick Start

```csharp
using Kck.Observability.Abstractions;

// Program.cs — the provider (Kck.Observability.OpenTelemetry)
builder.Services.AddKckObservability(obs => obs.UseOpenTelemetry(otel =>
{
    otel.ServiceName = "orders-api";
    otel.EnableTracing().EnableMetrics();
}));

// Tracing and metrics in your code
public class OrderService(ITracingService tracer, IMetricsService metrics)
{
    private readonly ICounter _ordersCreated = metrics.CreateCounter("orders.created", "Number of orders placed");

    public async Task CreateOrderAsync(Guid customerId, CancellationToken ct)
    {
        using var span = tracer.StartSpan("order.create");
        span.SetAttribute("customer.id", customerId.ToString());
        try
        {
            await Task.Delay(10, ct); // save the order
            _ordersCreated.Increment();
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

## Configuration

| Property | Description | Default |
|---|---|---|
| `Observability:ServiceName` | Service name in traces/metrics | Assembly name |
| `Observability:Otlp:Endpoint` | OTLP exporter endpoint | `"http://localhost:4317"` |
| `Observability:Tracing:SamplingRatio` | Head sampling ratio (0.0–1.0) | `1.0` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/observability.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
