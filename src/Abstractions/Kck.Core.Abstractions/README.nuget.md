# Kck.Core.Abstractions

Core domain primitives — `Entity<TId>` / `AuditableEntity<TId>` / `FullEntity<TId>` hierarchy, `Result<T>` / `Error` pattern, and `Paginate<T>` pagination.

## Installation

```bash
dotnet add package Kck.Core.Abstractions
```

## Quick Start

```csharp
// 1. Define a domain entity
public class Order : FullEntity<Guid>
{
    public string CustomerName { get; private set; } = string.Empty;
    public decimal Total { get; private set; }

    public static Order Create(string customerName, decimal total)
    {
        var order = new Order { CustomerName = customerName, Total = total };
        order.AddDomainEvent(new OrderCreatedEvent(order.Id));
        return order;
    }
}

// 2. Use the Result pattern in handlers
public async Task<Result<Order>> CreateOrderAsync(CreateOrderCommand cmd, CancellationToken ct)
{
    if (cmd.Total <= 0)
        return Result<Order>.Failure(Error.Validation("Total must be positive."));

    var order = Order.Create(cmd.CustomerName, cmd.Total);
    await _repo.AddAsync(order, ct);
    return Result<Order>.Success(order);
}

// 3. Return paginated results
var page = new Paginate<Order>(items, totalCount, pageIndex: 1, pageSize: 20);
```

## Configuration

This is a contracts-only package — no configuration required. Used as a transitive dependency by provider and bundle packages.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/persistence.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
