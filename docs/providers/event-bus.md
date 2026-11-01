# Event Bus

The `IEventBus` abstraction provides provider-agnostic publish/subscribe.
Event handlers are resolved from DI, with a transient scope per message.

## Packages

| Package | Purpose |
|---|---|
| `Kck.EventBus.Abstractions` | `IEventBus`, `IEventHandler<T>`, `IntegrationEvent` |
| `Kck.EventBus.InMemory` | In-process bus (handlers run on publish) |
| `Kck.EventBus.RabbitMq` | RabbitMQ (`RabbitMQ.Client`) |
| `Kck.EventBus.AzureServiceBus` | Azure Service Bus |

## InMemory

For tests and single-process apps. `PublishAsync` runs the handlers in the caller's flow, one DI scope per publish; a
handler exception is logged and swallowed. Nothing is persisted.

```csharp
services.AddKckEventBus(b => b
    .UseInMemory()
    .RegisterHandlersFromAssembly(typeof(OrderCreatedHandler).Assembly));
```

`RegisterHandlersFromAssembly` registers every `IEventHandler<T>` implementation of the assemblies as a transient
service (once, even when an assembly is scanned twice), and the handler class itself, and `PublishAsync` runs all of them. `Subscribe<TEvent, THandler>()` is optional on the in-memory bus: a
subscribed handler runs once, in addition to the registered ones.

## RabbitMQ

```csharp
services.AddKckEventBus(b => b.UseRabbitMq(opt =>
{
    opt.HostName = builder.Configuration["RabbitMq:Host"]!;
    opt.UserName = builder.Configuration["RabbitMq:User"]!;
    opt.Password = builder.Configuration["RabbitMq:Password"]!;
    opt.ExchangeName = "myapp.events";
}).RegisterHandlersFromAssembly(typeof(OrderCreatedHandler).Assembly));
```

Each subscribed event gets a durable queue `{QueuePrefix}.{EventName}` bound to the exchange.

| Option | Description | Default |
|---|---|---|
| `MaxDeliveryCount` | Deliveries of a failing message, the first one included, before it is rejected to the dead-letter queue; at least `1` | `5` |
| `PrefetchCount` | Unacknowledged messages the consumer takes from the broker at a time (`basic.qos`); `0` = unlimited | `10` |
| `DeadLetterEnabled` | Declare the dead-letter exchange and queue, and give consumer queues an `x-dead-letter-exchange` argument | `true` |
| `DeadLetterExchangeName` | Dead-letter exchange (fanout), one per service; `null` = `{QueuePrefix}.dlx` | `null` |
| `DeadLetterQueueName` | Dead-letter queue bound to it; `null` = `{QueuePrefix}.dlq` | `null` |

**Redelivery and dead-lettering.** A message whose handler throws is negatively acknowledged and requeued until it has
been delivered `MaxDeliveryCount` times; then it is rejected without requeue and the broker moves it to the
dead-letter queue. The dead-letter exchange is per service (`{QueuePrefix}.dlx`): services that share `ExchangeName`
do not receive each other's rejected messages. A handler's `OperationCanceledException`, for example an `HttpClient`
timeout, counts as a failure too; only host shutdown leaves the message unacknowledged for the broker to redeliver.
With `DeadLetterEnabled = false` the queues are declared without arguments and a rejected message is dropped unless a
broker policy sets a dead-letter exchange.

**Delivery counting.** The deliveries are counted in the consuming process, keyed by `MessageId`, because a requeued
message carries no delivery count on a classic queue. `MessageId` must therefore be unique per message; Kck sets it to
`IntegrationEvent.Id`. A message published by other code without a `MessageId` is delivered at most twice: the
broker's Redelivered flag counts as a previous delivery. The in-process count restarts when the consumer restarts or
another instance receives the message, so a scaled-out or restarting consumer can deliver a message more often.

**Redelivery is immediate.** There is no back-off between deliveries: the `MaxDeliveryCount` attempts happen within
milliseconds, so a transient outage of a dependency (a database failover, a downstream API restart) can dead-letter
every message delivered during it. Monitor the dead-letter queue. To redrive, inspect its messages (management UI),
fix the cause, then move them back to the consumer queue with a RabbitMQ shovel (the `rabbitmq_shovel` plugin; define
it in the management UI or with `rabbitmqctl set_parameter shovel`) or by republishing them.

**Queues declared by 4.x.** RabbitMQ refuses to re-declare a queue with other arguments. A consumer queue created by
4.x has none, so 5.0 stops at start-up with an `InvalidOperationException` (406 `PRECONDITION_FAILED`) that names the
queue. Use a new `QueuePrefix`, or set `DeadLetterEnabled = false` and apply the dead-letter exchange through a
policy, or delete the queue so it is re-declared (drain it first; its messages are lost). See
[the 4.x to 5.0 guide](../migrations/v4-to-v5.md).

**Channels, connection recovery and health.** A publish channel closed by the broker (a channel-level error) is
replaced on the next `PublishAsync`. After a connection loss the client's automatic recovery restores the connection,
the channels and the consumers; a `PublishAsync` during the recovery waits for it on the `RetryCount` / `RetryDelay`
schedule and does not open a second connection. A consumer channel closed by a channel-level error (for example a
`consumer_timeout` or a 406) is not reopened by the client: the bus logs a warning and its consumers stay stopped. The
`rabbitmq-eventbus` health check (tag `ready`) reports whether the bus is ready to consume and opens no connection of
its own. It is Unhealthy until the host has started the consumers, while the connection recovers, and after the
consumer channel was closed; restart the host in that last case.

## Azure Service Bus

```csharp
services.AddKckEventBus(b => b.UseAzureServiceBus(opt =>
{
    opt.ConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"]!;
    opt.TopicName = "order-events";
    opt.SubscriptionName = "order-processor";
}).RegisterHandlersFromAssembly(typeof(OrderCreatedHandler).Assembly));
```

To connect with a token credential (managed identity) or custom `ServiceBusClientOptions` instead of a connection
string, register your own `ServiceBusClient` in the container **before** `UseAzureServiceBus` (5.0), for example
`new ServiceBusClient("<namespace>.servicebus.windows.net", new DefaultAzureCredential())`. The bus then uses that
client, `ConnectionString` may stay empty (both the registration-time check and the start-up validation accept it),
and `RetryCount` is not applied: retries are the client's own options. The bus does not dispose a client it did not
create; it disposes its processors and its sender.

### Subscribing (RabbitMQ, Azure Service Bus)

The broker consumers start with the host and listen only for the events subscribed before that, so subscribe after
`Build()` and before `Run()`:

```csharp
using Kck.EventBus.Abstractions;

var bus = app.Services.GetRequiredService<IEventBus>();
bus.Subscribe<OrderCreatedEvent, OrderCreatedHandler>();
```

The handler must be registered in DI, through `RegisterHandlersFromAssembly` or as its own class
(`services.AddTransient<OrderCreatedHandler>()`); before 4.7 only the class registration was found. A handler that is
not registered is skipped with a log entry. A message whose handler throws is negatively acknowledged and redelivered;
after `MaxDeliveryCount` deliveries in total, the first one included, it goes to the dead-letter queue (RabbitMQ: see
[Redelivery and dead-lettering](#rabbitmq)).

## Defining an Event

```csharp
using Kck.EventBus.Abstractions;

public sealed record OrderCreatedEvent(Guid OrderId, decimal Amount) : IntegrationEvent;
```

## Defining a Handler

```csharp
using Kck.EventBus.Abstractions;
using Kck.Messaging.Abstractions;

public class OrderCreatedHandler(IEmailProvider email) : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct = default) =>
        email.SendAsync(new EmailMessage
        {
            From = "shop@example.com",
            Subject = $"Order {@event.OrderId} received",
            Body = $"Total: {@event.Amount:C}",
            Recipients = [new EmailRecipient("customer@example.com")],
        }, ct);
}
```

## Publish

```csharp
using Kck.EventBus.Abstractions;

public class OrderService(IEventBus bus)
{
    public async Task CreateAsync(Order order, CancellationToken ct)
    {
        // ... save the order first
        await bus.PublishAsync(new OrderCreatedEvent(order.Id, order.Total), ct);
    }
}
```

Publishing from a domain event handler waits for the commit, so a rolled-back order is never announced; see
[`OrderPlacedHandler.cs`](https://github.com/omerkck41/OmerkckArchitecture/blob/main/samples/Kck.Sample.Orders/Features/Orders/OrderPlacedHandler.cs)
in the reference app.

## At-Least-Once Semantics

- RabbitMq and AzureServiceBus: technically at-least-once
- Handlers must be **idempotent**, because the same event can be processed more than once
- An event ID plus processed-ID table pattern is recommended (outbox or inbox)

## Selection Criteria

| Criterion | InMemory | RabbitMq | AzureServiceBus |
|---|---|---|---|
| Persistence | None | Durable queue | Managed |
| Delivery | At-most-once | At-least-once | At-least-once |
| Dead letter | None | Yes | Yes |
| Scope | Single process | Cluster | Cloud-native |
| Best for | Test, CLI | Self-hosted messaging | Azure workload |
