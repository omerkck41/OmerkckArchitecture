using Kck.Core.Abstractions.Entities;

namespace Kck.Sample.Orders.Domain;

/// <summary>
/// A customer. Soft-deletable; orders reference customers with <c>DeleteBehavior.Restrict</c>, so a customer with
/// orders cannot be deleted (OrdersDbContext, CustomerEndpoints).
/// </summary>
public sealed class Customer : FullEntity<Guid>
{
    public const int NameMaxLength = 200;

    public required string Name { get; set; }
}
