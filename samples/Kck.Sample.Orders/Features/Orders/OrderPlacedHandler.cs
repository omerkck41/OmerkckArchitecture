using Kck.Core.Abstractions.Entities;
using Kck.EventBus.Abstractions;
using Kck.Sample.Orders.Domain;

namespace Kck.Sample.Orders.Features.Orders;

/// <summary>Asks for an order confirmation; published to the event bus by <see cref="OrderPlacedHandler"/>.</summary>
public sealed record OrderConfirmationRequested(Guid OrderId, Guid CustomerId, decimal Total) : IntegrationEvent;

/// <summary>
/// Runs after an order is committed (never for a rolled-back order) and turns the domain event into an integration
/// event. With the in-memory bus the handlers run in this process; with RabbitMQ or Azure Service Bus other services
/// receive it. Delivery is at least once, so handlers must be idempotent.
/// </summary>
public sealed partial class OrderPlacedHandler(IEventBus eventBus, ILogger<OrderPlacedHandler> logger)
    : IDomainEventHandler<OrderPlaced>
{
    public async Task HandleAsync(OrderPlaced domainEvent, CancellationToken cancellationToken)
    {
        LogOrderPlaced(domainEvent.OrderId, domainEvent.CustomerId, domainEvent.Total);
        await eventBus.PublishAsync(
            new OrderConfirmationRequested(domainEvent.OrderId, domainEvent.CustomerId, domainEvent.Total),
            cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} placed by customer {CustomerId}, total {Total}")]
    private partial void LogOrderPlaced(Guid orderId, Guid customerId, decimal total);
}

/// <summary>
/// Stands in for an e-mail sender: logs the confirmation instead of sending it, so the sample needs no mail provider.
/// Replace it with Kck.Messaging (MailKit, SendGrid, Amazon SES) in a real application.
/// </summary>
public sealed partial class SendOrderConfirmation(ILogger<SendOrderConfirmation> logger) : IEventHandler<OrderConfirmationRequested>
{
    public Task HandleAsync(OrderConfirmationRequested @event, CancellationToken ct = default)
    {
        LogConfirmation(@event.OrderId, @event.CustomerId);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Order confirmation for order {OrderId} sent to customer {CustomerId}")]
    private partial void LogConfirmation(Guid orderId, Guid customerId);
}
