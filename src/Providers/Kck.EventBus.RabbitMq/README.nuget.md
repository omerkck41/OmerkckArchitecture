# Kck.EventBus.RabbitMq

RabbitMQ-backed `IEventBus` for durable, topic-exchange-based event publishing and consumption across distributed services.

## Installation

```bash
dotnet add package Kck.EventBus.RabbitMq
```

## Quick Start

```csharp
using Kck.EventBus.Abstractions;

builder.Services.AddKckEventBus(eb => eb
    .UseRabbitMq(options =>
    {
        options.HostName = "localhost";
        options.UserName = "guest";
        options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")!;
        options.ExchangeName = "domain-events";
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
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `HostName` | Broker host name (required) | `""` |
| `Port` | AMQP port | `5672` |
| `UserName` / `Password` | Broker credentials | `""` |
| `VirtualHost` | Virtual host | `"/"` |
| `ExchangeName` | Exchange the events are published to | `"kck.eventbus"` |
| `ExchangeType` | Exchange type | `"topic"` |
| `QueuePrefix` | Consumer queues are `{QueuePrefix}.{EventName}` | `"kck"` |
| `MaxDeliveryCount` | Deliveries of a failing message before it is dead-lettered | `5` |
| `RetryCount` / `RetryDelay` / `MaxRetryDelay` | Connection retry policy | `5` / `2 s` / `30 s` |

Queues are durable and a message is acknowledged after its handlers succeed (at-least-once): handlers must be
idempotent.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
