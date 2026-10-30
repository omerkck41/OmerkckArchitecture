# Kck.EventBus.AzureServiceBus

Azure Service Bus provider for `IEventBus` — publish and subscribe to domain events via topics and subscriptions.

## Installation

```bash
dotnet add package Kck.EventBus.AzureServiceBus
```

## Quick Start

```csharp
using Kck.EventBus.Abstractions;

// Program.cs
builder.Services.AddKckEventBus(eb => eb
    .UseAzureServiceBus(options =>
    {
        options.ConnectionString = Environment.GetEnvironmentVariable("ASB_CONNECTION")!;
        options.TopicName = "domain-events";
        options.SubscriptionName = "my-service";
    })
    .RegisterHandlersFromAssembly(typeof(OrderPlacedHandler).Assembly));

var app = builder.Build();

// Consumers start with the host: subscribe before Run().
app.Services.GetRequiredService<IEventBus>().Subscribe<OrderPlacedEvent, OrderPlacedHandler>();

public sealed record OrderPlacedEvent(Guid OrderId) : IntegrationEvent;

public sealed class OrderPlacedHandler : IEventHandler<OrderPlacedEvent>
{
    public Task HandleAsync(OrderPlacedEvent eventData, CancellationToken ct = default) => Task.CompletedTask;
}

// Publish after the data is saved: bus.PublishAsync(new OrderPlacedEvent(order.Id), ct)
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ConnectionString` | Service Bus namespace connection string | required |
| `TopicName` | Topic the events are published to | `"kck-eventbus"` |
| `SubscriptionName` | Subscription the consumer reads | `"default"` |
| `MaxConcurrentCalls` | Messages processed in parallel | `10` |
| `MaxAutoLockRenewalDuration` | Lock renewal limit for a message | 5 minutes |
| `RetryCount` | Client retry count | `3` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
