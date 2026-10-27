# Kck.Persistence.Abstractions

Repository and Unit-of-Work persistence abstractions — `IReadRepository<T,TId>`, `IWriteRepository<T,TId>`,
`QueryOptions`, dynamic filtering and `IUnitOfWork`. The Entity Framework implementation is
`Kck.Persistence.EntityFramework`.

## Installation

```bash
dotnet add package Kck.Persistence.Abstractions
dotnet add package Kck.Persistence.EntityFramework
```

## Quick Start

```csharp
// Program.cs — register the Entity Framework provider
builder.Services.AddKckPersistence(p => p
    .UseEntityFramework<AppDbContext>(o => o
        .UseSqlServer(builder.Configuration.GetConnectionString("Default"))
        .AddInterceptors(new AuditInterceptor())));

// Repositories come from the unit of work
public sealed class OrderHandlers(IUnitOfWork uow)
{
    public Task<Order?> GetAsync(Guid orderId, CancellationToken ct)
        => uow.Repository<Order, Guid>().GetByIdAsync(orderId, cancellationToken: ct);

    public async Task<Order> CreateAsync(Order order, CancellationToken ct)
    {
        await uow.Repository<Order, Guid>().AddAsync(order, ct);
        await uow.SaveChangesAsync(cancellationToken: ct);
        return order;
    }

    // Filtered, ordered page; QueryOptions controls soft-deleted rows and change tracking
    public Task<IPaginate<Order>> ActiveAsync(CancellationToken ct)
        => uow.Repository<Order, Guid>().GetListAsync(
            QueryOptions.None,
            predicate: o => o.Total > 0,
            orderBy: q => q.OrderByDescending(o => o.CreatedDate),
            size: 20,
            cancellationToken: ct);
}
```

When `options` is omitted, `GetAsync` / `GetListAsync` / `GetListByDynamicAsync` track entities and `AnyAsync` /
`GetByIdAsync` / `CountAsync` do not. `QueryOptions.WithDeleted` does not track; to load a soft-deleted row and save
changes to it, pass `new QueryOptions(IncludeDeleted: true, AsTracking: true)`.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/persistence.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
