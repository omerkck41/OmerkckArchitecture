# Kck.Core.Abstractions

Core domain primitives — `Entity<TId>` / `AuditableEntity<TId>` / `FullEntity<TId>` hierarchy, `Result<T>` / `Error` pattern, and `Paginate<T>` pagination.

## Installation

```bash
dotnet add package Kck.Core.Abstractions
```

## Quick Start

```csharp
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;
using Kck.Core.Abstractions.Results;

// 1. Define a domain entity that raises a domain event
public class Order : FullEntity<Guid>
{
    public string CustomerName { get; private set; } = string.Empty;
    public decimal Total { get; private set; }

    public static Order Create(string customerName, decimal total)
    {
        var order = new Order { Id = Guid.CreateVersion7(), CustomerName = customerName, Total = total };
        order.AddDomainEvent(new OrderCreated(order.Id));
        return order;
    }
}

public sealed record OrderCreated(Guid OrderId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public class OrderFactory
{
    // 2. Return a Result instead of throwing for expected failures
    public Result<Order> Create(string customerName, decimal total) =>
        total <= 0
            ? Result<Order>.Failure(new Error("order.total", "Total must be positive.", ErrorType.Validation))
            : Result<Order>.Success(Order.Create(customerName, total));

    // 3. Build a page from data you already loaded (repositories return IPaginate<T> for you)
    public IPaginate<Order> FirstPage(IReadOnlyList<Order> items, int totalCount) =>
        Paginate<Order>.Create(items, totalCount, index: 0, size: 20);
}
```

## Configuration

This is a contracts-only package — no configuration required. Used as a transitive dependency by provider and bundle packages.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/persistence.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
