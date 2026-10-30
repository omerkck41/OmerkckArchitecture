# Event Bus

The `IEventBus` abstraction provides provider-agnostic publish/subscribe.
Event handlers are resolved from DI, with a transient scope per message.

## Packages

| Package | Purpose |
|---|---|
| `Kck.EventBus.Abstractions` | `IEventBus`, `IEventHandler<T>`, `IntegrationEvent` |
| `Kck.EventBus.InMemory` | In-process channel-based bus |
| `Kck.EventBus.RabbitMq` | RabbitMQ (`RabbitMQ.Client`) |
| `Kck.EventBus.AzureServiceBus` | Azure Service Bus |

## InMemory

For testing and single-process scenarios. Asynchronous queue; there is no DLQ on failure.

```csharp
services.AddKckEventBus(b => b
    .UseInMemory()
    .RegisterHandlersFromAssembly(typeof(OrderCreatedHandler).Assembly));
```

## RabbitMQ

```csharp
services.AddKckEventBusRabbitMq(opt =>
{
    opt.HostName = builder.Configuration["RabbitMq:Host"]!;
    opt.UserName = builder.Configuration["RabbitMq:User"]!;
    opt.Password = builder.Configuration["RabbitMq:Password"]!;
    opt.Exchange = "myapp.events";
})
.AddKckEventHandler<OrderCreatedEvent, OrderCreatedHandler>();
```

## Azure Service Bus

```csharp
services.AddKckEventBusAzureServiceBus(opt =>
{
    opt.ConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"]!;
    opt.TopicName = "order-events";
    opt.SubscriptionName = "order-processor";
})
.AddKckEventHandler<OrderCreatedEvent, OrderCreatedHandler>();
```

## Defining an Event

```csharp
public sealed record OrderCreatedEvent(Guid OrderId, decimal Amount) : IntegrationEvent;
```

## Defining a Handler

```csharp
public class OrderCreatedHandler(IEmailService email) : IEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct = default)
    {
        await email.SendAsync(/* ... */, ct);
    }
}
```

## Publish

```csharp
public class OrderService(IEventBus bus)
{
    public async Task CreateAsync(Order order, CancellationToken ct)
    {
        // ...
        await bus.PublishAsync(new OrderCreatedEvent(order.Id, order.Amount), ct);
    }
}
```

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
