# Kck.Persistence.EntityFramework

Entity Framework Core integration for Kck applications — registers `IUnitOfWork` (repositories via `IUnitOfWork.Repository<T, TId>()`), audit / soft-delete and domain event interceptors, and dynamic query extensions.

New to Kck? [Your first Kck API in five minutes](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/getting-started/README.md) creates a working API with this package: `dotnet new install Kck.Templates`, then `dotnet new kck-webapi`.

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
| `AddDomainEventHandlers(assemblies)` | Registers every `IDomainEventHandler<TEvent>` in the assemblies and dispatches domain events to them after `SaveChanges` / commit (4.6.0+) |
| `AddDomainEventDispatch<TDispatcher>()` | Dispatches aggregate domain events after `SaveChanges` / commit through your own `IDomainEventDispatcher` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/persistence.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
