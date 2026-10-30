using Kck.Core.Abstractions.Entities;

namespace KckWebApiTemplate.Api.Orders;

/// <summary>
/// Runs after an order is committed (never for a rolled-back order). Replace the log line with real work — send a
/// confirmation, publish an integration event. Delivery is at least once, so keep the work idempotent.
/// </summary>
public sealed partial class OrderPlacedHandler(ILogger<OrderPlacedHandler> logger) : IDomainEventHandler<OrderPlaced>
{
    public Task HandleAsync(OrderPlaced domainEvent, CancellationToken cancellationToken)
    {
        LogOrderPlaced(domainEvent.OrderId, domainEvent.CustomerId, domainEvent.Total);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} placed by customer {CustomerId}, total {Total}")]
    private partial void LogOrderPlaced(Guid orderId, Guid customerId, decimal total);
}
