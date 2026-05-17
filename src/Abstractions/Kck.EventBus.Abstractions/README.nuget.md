# Kck.EventBus.Abstractions

Event-driven messaging abstractions with pub/sub pattern — `IEventBus`, `IEventHandler<TEvent>`, and `IntegrationEvent` base record for InMemory, RabbitMQ, or Azure Service Bus providers.

## Installation

```bash
dotnet add package Kck.EventBus.Abstractions
```

## Quick Start

```csharp
// 1. Define an integration event
public record OrderPlacedEvent(Guid OrderId, string CustomerEmail) : IntegrationEvent;

// 2. Implement a handler
public class SendOrderConfirmationHandler(IEmailService email)
    : IEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent @event, CancellationToken ct)
        => await email.SendAsync(new EmailMessage
        {
            To = @event.CustomerEmail,
            Subject = "Order confirmed",
            Body = $"Your order {@event.OrderId} has been placed."
        }, ct);
}

// Program.cs — register provider + subscribe handlers
builder.Services.AddKckInMemoryEventBus(bus =>
{
    bus.Subscribe<OrderPlacedEvent, SendOrderConfirmationHandler>();
});

// Publish from a handler/service
public class PlaceOrderHandler(IEventBus eventBus)
{
    public async Task HandleAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        // ... save order ...
        await eventBus.PublishAsync(new OrderPlacedEvent(order.Id, cmd.CustomerEmail), ct);
    }
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
