using Kck.Core.Abstractions.Entities;

namespace Kck.Sample.Orders.Domain;

/// <summary>A catalogue product; its id is the SKU. Stock is reserved when an order is placed.</summary>
public sealed class Product : Entity<string>
{
    public const int SkuMaxLength = 64;

    public required string Name { get; set; }

    public decimal UnitPrice { get; set; }

    public int Stock { get; set; }
}
