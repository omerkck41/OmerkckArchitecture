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

## Azure Service Bus

```csharp
services.AddKckEventBus(b => b.UseAzureServiceBus(opt =>
{
    opt.ConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"]!;
    opt.TopicName = "order-events";
    opt.SubscriptionName = "order-processor";
}).RegisterHandlersFromAssembly(typeof(OrderCreatedHandler).Assembly));
```

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
not registered is skipped with a log entry. A message whose handler throws is negatively acknowledged and redelivered up to
`MaxDeliveryCount` (RabbitMQ) before it goes to the dead-letter queue.

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
