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
| `MaxDeliveryCount` | Deliveries of a failing message, the first one included, before it is rejected to the dead-letter queue; at least `1` | `5` |
| `PrefetchCount` | Unacknowledged messages the consumer takes from the broker at a time (`basic.qos`); `0` = unlimited | `10` |
| `DeadLetterEnabled` | Declare the dead-letter exchange and queue, and give consumer queues an `x-dead-letter-exchange` argument | `true` |
| `DeadLetterExchangeName` | Dead-letter exchange (fanout), one per service; `null` = `{QueuePrefix}.dlx` | `null` |
| `DeadLetterQueueName` | Dead-letter queue bound to it; `null` = `{QueuePrefix}.dlq` | `null` |
| `RetryCount` / `RetryDelay` / `MaxRetryDelay` | Connection retry policy | `5` / `2 s` / `30 s` |

Queues are durable and a message is acknowledged after its handlers succeed (at-least-once): handlers must be
idempotent.

## Redelivery and dead-lettering

A message whose handler throws is negatively acknowledged and requeued until it has been delivered
`MaxDeliveryCount` times; then it is rejected without requeue and the broker moves it to the dead-letter queue
(`{QueuePrefix}.dlq`, behind the per-service fanout exchange `{QueuePrefix}.dlx`). A handler's
`OperationCanceledException`, for example an `HttpClient` timeout, counts as a failure too; only host shutdown leaves
the message unacknowledged for the broker to redeliver.

The deliveries are counted in the consuming process, keyed by `MessageId`, because a requeued message carries no
delivery count on a classic queue. `MessageId` must therefore be unique per message; Kck sets it to
`IntegrationEvent.Id`. A message published by other code without a `MessageId` is delivered at most twice: the
broker's Redelivered flag counts as a previous delivery. The in-process count restarts when the consumer restarts or
another instance receives the message, so a scaled-out or restarting consumer can deliver a message more often.

Redelivery is immediate, without back-off: the `MaxDeliveryCount` deliveries happen within milliseconds, so a transient
outage of a dependency can dead-letter every message delivered during it. Watch the dead-letter queue and, once the
cause is fixed, move its messages back to the consumer queue with a RabbitMQ shovel (`rabbitmq_shovel` plugin; define
it in the management UI or with `rabbitmqctl set_parameter shovel`) or by republishing them.

With `DeadLetterEnabled = false` the queues are declared without arguments and a rejected message is dropped unless a
broker policy sets a dead-letter exchange. Consumer queues declared by 4.x have no arguments, so 5.0 fails at start-up
with a 406 `PRECONDITION_FAILED` error naming the queue: use a new `QueuePrefix`, or set `DeadLetterEnabled = false`
and apply the dead-letter exchange through a policy, or delete the queue so it is re-declared (drain it first).

A publish channel closed by the broker (a channel-level error) is replaced on the next `PublishAsync`. After a
connection loss the client recovers the connection, channels and consumers on its own; a `PublishAsync` during the
recovery waits for it (on the `RetryCount` / `RetryDelay` schedule) instead of opening a second connection.

The `rabbitmq-eventbus` health check (tag `ready`) reports whether the bus is ready to consume; it opens no connection
of its own. It is Unhealthy until the host has started the consumers, while the connection recovers, and when a
channel-level error (for example a `consumer_timeout` or a 406) closed the consumer channel, which the client does not
reopen: restart the host then.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
