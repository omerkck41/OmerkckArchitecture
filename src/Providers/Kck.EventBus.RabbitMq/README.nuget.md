# Kck.EventBus.RabbitMq

RabbitMQ-backed `IEventBus` for durable, topic-exchange-based event publishing and consumption across distributed services.

## Installation

```bash
dotnet add package Kck.EventBus.RabbitMq
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckEventBus(eb =>
    eb.UseRabbitMq(options =>
    {
        options.HostName = "localhost";
        options.Port = 5672;
        options.UserName = "guest";
        options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")!;
        options.ExchangeName = "domain-events";
    }));

// Subscribe to an event
builder.Services.AddKckEventHandler<OrderPlacedEvent, OrderPlacedHandler>();
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `HostName` | RabbitMQ broker hostname | `"localhost"` |
| `Port` | AMQP port | `5672` |
| `UserName` | Broker username | `"guest"` |
| `Password` | Broker password | `"guest"` |
| `ExchangeName` | Topic exchange name | `"kck-events"` |
| `VirtualHost` | RabbitMQ virtual host | `"/"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
