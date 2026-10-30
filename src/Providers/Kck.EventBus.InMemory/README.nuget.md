# Kck.EventBus.InMemory

In-process `IEventBus` implementation for single-node applications, integration tests, and local development.

## Installation

```bash
dotnet add package Kck.EventBus.InMemory
```

## Quick Start

```csharp
using Kck.EventBus.Abstractions;

builder.Services.AddKckEventBus(eb => eb
    .UseInMemory()
    .RegisterHandlersFromAssembly(typeof(OrderPlacedHandler).Assembly));

public sealed record OrderPlacedEvent(Guid OrderId) : IntegrationEvent;

public sealed class OrderPlacedHandler(ILogger<OrderPlacedHandler> logger) : IEventHandler<OrderPlacedEvent>
{
    public Task HandleAsync(OrderPlacedEvent eventData, CancellationToken ct = default)
    {
        logger.LogInformation("Order {OrderId} placed", eventData.OrderId);
        return Task.CompletedTask;
    }
}

// Publish after the data is saved
public class OrderService(IEventBus bus)
{
    public Task PlaceAsync(Guid orderId, CancellationToken ct) => bus.PublishAsync(new OrderPlacedEvent(orderId), ct);
}
```

`PublishAsync` runs every registered handler in the caller's flow, in one DI scope; a handler exception is logged and
not rethrown. Nothing is persisted: use a broker provider when events must survive a restart or cross processes. The
bus has no options.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
