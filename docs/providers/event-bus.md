# Event Bus

`IEventBus` abstraction'i, provider-agnostic publish/subscribe destegidir.
Event handler'lar DI'dan cozulur, transient scope per mesaj.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.EventBus.Abstractions` | `IEventBus`, `IEventHandler<T>`, `IntegrationEvent` |
| `Kck.EventBus.InMemory` | In-process channel-based bus |
| `Kck.EventBus.RabbitMq` | RabbitMQ (`RabbitMQ.Client`) |
| `Kck.EventBus.AzureServiceBus` | Azure Service Bus |

## InMemory

Testing ve tek-process senaryolar icin. Asenkron queue, hata durumunda DLQ yok.

```csharp
services.AddKckEventBusInMemory()
    .AddKckEventHandler<OrderCreatedEvent, OrderCreatedHandler>();
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

## Event Tanimi

```csharp
public sealed record OrderCreatedEvent(Guid OrderId, decimal Amount) : IntegrationEvent;
```

## Handler Tanimi

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

## At-Least-Once Semantigi

- RabbitMq ve AzureServiceBus: teknik olarak at-least-once
- Handler'lar **idempotent** olmali — ayni event birden fazla islenebilir
- Event ID + processed-id tablosu pattern onerilir (outbox veya inbox)

## Secim Kriterleri

| Kriter | InMemory | RabbitMq | AzureServiceBus |
|---|---|---|---|
| Persistence | Yok | Durable queue | Managed |
| Delivery | At-most-once | At-least-once | At-least-once |
| Dead letter | Yok | Evet | Evet |
| Scope | Tek process | Cluster | Cloud-native |
| Best for | Test, CLI | Self-hosted mesajlasma | Azure workload |
