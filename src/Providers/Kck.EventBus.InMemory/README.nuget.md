# Kck.EventBus.InMemory

In-process `IEventBus` implementation for single-node applications, integration tests, and local development.

## Installation

```bash
dotnet add package Kck.EventBus.InMemory
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckEventBus(eb => eb.UseInMemory());

// Subscribe to an event
builder.Services.AddKckEventHandler<OrderPlacedEvent, OrderPlacedHandler>();

// Publish an event
public class OrderService(IEventBus bus)
{
    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
        await bus.PublishAsync(new OrderPlacedEvent(order.Id), ct);
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ThrowOnHandlerError` | Re-throw exceptions from event handlers | `false` |
| `MaxConcurrentHandlers` | Maximum parallel handler invocations | `Environment.ProcessorCount` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
