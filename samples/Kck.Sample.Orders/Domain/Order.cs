using Kck.Core.Abstractions.Entities;

namespace Kck.Sample.Orders.Domain;

public enum OrderStatus
{
    Placed,
    Paid,
    Shipped,
}

/// <summary>
/// An order: <see cref="OrderStatus.Placed"/> → <see cref="OrderStatus.Paid"/> → <see cref="OrderStatus.Shipped"/>.
/// Soft-deletable: cancelling marks the order and, through the cascade relationship, its lines as deleted. Placing an
/// order raises <see cref="OrderPlaced"/>.
/// </summary>
public sealed class Order : FullEntity<Guid>
{
    private Order()
    {
    }

    public Guid CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public List<OrderLine> Lines { get; private set; } = [];

    /// <summary>Order total, computed in memory: SQLite cannot sum decimal columns in SQL.</summary>
    public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);

    /// <summary>A shipped order is with the carrier; it can no longer be cancelled.</summary>
    public bool CanCancel => Status != OrderStatus.Shipped;

    public static Order Place(Guid customerId, IEnumerable<OrderLine> lines)
    {
        var order = new Order { Id = Guid.CreateVersion7(), CustomerId = customerId, Status = OrderStatus.Placed };
        order.Lines.AddRange(lines);
        order.AddDomainEvent(new OrderPlaced(order.Id, customerId, order.Total));
        return order;
    }

    /// <summary>Moves a placed order to paid; returns <c>false</c> from any other status.</summary>
    public bool TryPay() => TryMove(OrderStatus.Placed, OrderStatus.Paid);

    /// <summary>Moves a paid order to shipped; returns <c>false</c> from any other status.</summary>
    public bool TryShip() => TryMove(OrderStatus.Paid, OrderStatus.Shipped);

    private bool TryMove(OrderStatus from, OrderStatus to)
    {
        if (Status != from)
            return false;

        Status = to;
        return true;
    }
}

/// <summary>An order line; the unit price is copied from the catalogue when the order is placed.</summary>
public sealed class OrderLine : FullEntity<Guid>
{
    private OrderLine()
    {
    }

    public Guid OrderId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public static OrderLine For(Product product, int quantity) =>
        new() { Id = Guid.CreateVersion7(), Sku = product.Id, Quantity = quantity, UnitPrice = product.UnitPrice };
}

/// <summary>Raised when an order is placed; dispatched only after the order is committed.</summary>
public sealed record OrderPlaced(Guid OrderId, Guid CustomerId, decimal Total) : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();

    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
