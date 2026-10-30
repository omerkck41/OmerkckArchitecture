# Kck.EventBus.Abstractions

Event-driven messaging abstractions with pub/sub pattern — `IEventBus`, `IEventHandler<TEvent>`, and `IntegrationEvent` base record for InMemory, RabbitMQ, or Azure Service Bus providers.

## Installation

```bash
dotnet add package Kck.EventBus.Abstractions
```

## Quick Start

```csharp
using Kck.EventBus.Abstractions;

// Program.cs — a provider (InMemory, RabbitMq, AzureServiceBus) and the handlers of this assembly
builder.Services.AddKckEventBus(bus => bus
    .UseInMemory()
    .RegisterHandlersFromAssembly(typeof(OrderPlacedHandler).Assembly));

// 1. An integration event
public sealed record OrderPlacedEvent(Guid OrderId, string CustomerEmail) : IntegrationEvent;

// 2. A handler
public class OrderPlacedHandler(ILogger<OrderPlacedHandler> logger) : IEventHandler<OrderPlacedEvent>
{
    public Task HandleAsync(OrderPlacedEvent eventData, CancellationToken ct = default)
    {
        logger.LogInformation("Order {OrderId} placed", eventData.OrderId);
        return Task.CompletedTask;
    }
}

// 3. Publish after the data is saved
public class PlaceOrderService(IEventBus eventBus)
{
    public Task PlacedAsync(Guid orderId, string customerEmail, CancellationToken ct) =>
        eventBus.PublishAsync(new OrderPlacedEvent(orderId, customerEmail), ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `EventBus:Provider` | `InMemory`, `RabbitMq`, `AzureServiceBus` | `InMemory` |
| `EventBus:RetryCount` | Publish retry attempts on transient errors | `3` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
