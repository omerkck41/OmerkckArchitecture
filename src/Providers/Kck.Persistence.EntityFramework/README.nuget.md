# Kck.Persistence.EntityFramework

Entity Framework Core integration for Kck applications — registers `IRepository<T>`, `IUnitOfWork`, and dynamic query extensions.

## Installation

```bash
dotnet add package Kck.Persistence.EntityFramework
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckEntityFramework<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")!));

// Use repository
public class OrderService(IRepository<Order> orders, IUnitOfWork uow)
{
    public async Task<Order> CreateAsync(Order order, CancellationToken ct)
    {
        await orders.AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        return order;
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `DbContextOptions` | EF Core `DbContextOptionsBuilder` delegate | required |
| `EnableSensitiveDataLogging` | Log parameter values (dev only) | `false` |
| `EnableDetailedErrors` | Include EF error details | `false` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/persistence.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
