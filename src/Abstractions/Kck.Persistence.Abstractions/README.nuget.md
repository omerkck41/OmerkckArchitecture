# Kck.Persistence.Abstractions

Repository and Unit-of-Work persistence abstractions following the Specification pattern — `IReadRepository<T,TId>`, `IWriteRepository<T,TId>`, dynamic filtering, and `IUnitOfWork`.

## Installation

```bash
dotnet add package Kck.Persistence.Abstractions
```

## Quick Start

```csharp
// Program.cs — register the Entity Framework provider
builder.Services.AddKckEntityFramework<AppDbContext>(builder.Configuration);

// Use repositories in handlers
public class GetOrderHandler(IReadRepository<Order, Guid> orders)
{
    public async Task<Order?> HandleAsync(Guid orderId, CancellationToken ct)
        => await orders.GetByIdAsync(orderId, ct);
}

public class CreateOrderHandler(
    IWriteRepository<Order, Guid> orders, IUnitOfWork uow)
{
    public async Task<Order> HandleAsync(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerName, cmd.Total);
        await orders.AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        return order;
    }
}

// Dynamic query with QueryOptions
var activeOrders = await orders.ListAsync(
    new QueryOptions<Order>
    {
        Filter       = o => o.Status == OrderStatus.Active,
        OrderBy      = q => q.OrderByDescending(o => o.CreatedAt),
        AsTracking   = false
    }, ct);
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Persistence:ConnectionString` | Database connection string | — |
| `Persistence:CommandTimeout` | EF command timeout (seconds) | `30` |
| `Persistence:EnableSensitiveDataLogging` | Log parameter values | `false` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/persistence.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
