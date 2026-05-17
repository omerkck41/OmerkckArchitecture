# Kck.Observability.Abstractions

Observability abstractions — distributed tracing (`ITracingService`), structured metrics (`IMetricsService`), and health checks (`IHealthCheck`) backed by the OpenTelemetry provider.

## Installation

```bash
dotnet add package Kck.Observability.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider (Kck.Observability.OpenTelemetry)
builder.Services.AddKckOpenTelemetry(builder.Configuration);

// Tracing — wrap operations in spans
public class OrderService(ITracingService tracer, IMetricsService metrics)
{
    private readonly ICounter _ordersCreated =
        metrics.CreateCounter("orders.created", "Number of orders placed");

    public async Task<Order> CreateOrderAsync(CreateOrderCommand cmd, CancellationToken ct)
    {
        using var span = tracer.StartSpan("order.create");
        span.SetTag("customer.id", cmd.CustomerId.ToString());
        try
        {
            var order = await _repo.AddAsync(Order.Create(cmd), ct);
            _ordersCreated.Increment();
            span.SetStatus(SpanStatus.Ok);
            return order;
        }
        catch (Exception ex)
        {
            span.RecordException(ex);
            span.SetStatus(SpanStatus.Error);
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
