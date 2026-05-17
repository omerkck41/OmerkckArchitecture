# Kck.EventBus.AzureServiceBus

Azure Service Bus provider for `IEventBus` — publish and subscribe to domain events via topics and subscriptions.

## Installation

```bash
dotnet add package Kck.EventBus.AzureServiceBus
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckEventBus(eb =>
    eb.UseAzureServiceBus(options =>
    {
        options.ConnectionString = Environment.GetEnvironmentVariable("ASB_CONNECTION")!;
        options.TopicName = "domain-events";
        options.SubscriptionName = "my-service";
    }));

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
| `ConnectionString` | Azure Service Bus namespace connection string | required |
| `TopicName` | Topic name for publishing events | `"events"` |
| `SubscriptionName` | Subscription name for consuming events | required |
| `MaxConcurrentMessages` | Parallel message processing limit | `1` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
