# Kck.Persistence.EntityFramework

Entity Framework Core integration for Kck applications — registers `IUnitOfWork` (repositories via `IUnitOfWork.Repository<T, TId>()`), audit / soft-delete and domain event interceptors, and dynamic query extensions.

## Installation

```bash
dotnet add package Kck.Persistence.EntityFramework
```

## Quick Start

```csharp
// Program.cs
using Kck.Persistence.EntityFramework.Extensions;

builder.Services.AddKckPersistence(p => p
    .UseEntityFramework<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")!))
    .AddAuditInterceptor());   // CreatedDate / ModifiedDate, DbContext.Remove -> soft delete

// Use a repository through the unit of work
public class OrderService(IUnitOfWork uow)
{
    public async Task<Order> CreateAsync(Order order, CancellationToken ct)
    {
        await uow.Repository<Order, Guid>().AddAsync(order, ct);
        await uow.SaveChangesAsync(cancellationToken: ct);
        return order;
    }
}
```

## Configuration

| Method | Description |
|---|---|
| `UseEntityFramework<TContext>(options)` | Registers the `DbContext` (`AddDbContext`) and `IUnitOfWork` |
| `AddAuditInterceptor()` | Audit fields and soft delete for `IAuditable` / `ISoftDeletable` entities |
| `AddDomainEventDispatch<TDispatcher>()` | Dispatches aggregate domain events after `SaveChanges` / commit |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/persistence.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
