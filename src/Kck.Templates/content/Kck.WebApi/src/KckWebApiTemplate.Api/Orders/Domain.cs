using Kck.Core.Abstractions.Entities;

namespace KckWebApiTemplate.Api.Orders;

/// <summary>A customer. Orders reference customers; cancelling an order never touches the customer.</summary>
public sealed class Customer : FullEntity<Guid>
{
    public required string Name { get; set; }
}

/// <summary>A catalogue product; its id is the SKU. Stock is reserved when an order is placed.</summary>
public sealed class Product : Entity<string>
{
    public const int SkuMaxLength = 64;

    public required string Name { get; set; }

    public decimal UnitPrice { get; set; }

    public int Stock { get; set; }
}

/// <summary>
/// An order. Soft-deletable (<see cref="FullEntity{TId}"/>): cancelling marks the order and, through the cascade
/// relationship, its lines as deleted. Placing an order raises <see cref="OrderPlaced"/>.
/// </summary>
public sealed class Order : FullEntity<Guid>
{
    private Order()
    {
    }

    public Guid CustomerId { get; private set; }

    public List<OrderLine> Lines { get; private set; } = [];

    /// <summary>Order total, computed in memory: SQLite cannot sum decimal columns in SQL.</summary>
    public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);

    public static Order Place(Guid customerId, IEnumerable<OrderLine> lines)
    {
        var order = new Order { Id = Guid.CreateVersion7(), CustomerId = customerId };
        order.Lines.AddRange(lines);
        order.AddDomainEvent(new OrderPlaced(order.Id, customerId, order.Total));
        return order;
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
