# Kck.Exceptions.Abstractions

Structured HTTP-aware exception hierarchy — `CustomException` base with concrete types that map automatically to HTTP status codes via `[HttpStatusCode]` attribute.

## Installation

```bash
dotnet add package Kck.Exceptions.Abstractions
```

## Quick Start

```csharp
using Kck.Exceptions;

// Throw typed exceptions in the domain/application layer
public class OrderService(IOrderStore orders, IPermissionChecker permissions)
{
    public async Task<Order> GetOrderAsync(Guid id, CancellationToken ct) =>
        await orders.FindAsync(id, ct) ?? throw new NotFoundException($"Order {id} was not found.");

    public async Task UpdateOrderAsync(Guid userId, Order order, CancellationToken ct)
    {
        if (order.Total <= 0)
            throw new BadRequestException("Order total must be greater than zero.");

        if (!await permissions.HasPermissionAsync(userId, "orders.write", ct))
            throw new ForbiddenException("You do not have permission to update orders.");
    }
}

public interface IOrderStore
{
    Task<Order?> FindAsync(Guid id, CancellationToken ct);
}

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct);
}

// The exception middleware (Kck.Exceptions.AspNetCore) catches these and returns the matching status code:
// NotFoundException 404, BadRequestException 400, ForbiddenException 403.
```

## Configuration

This is a contracts-only package — no configuration required. Pair with `Kck.Exceptions.AspNetCore` for automatic HTTP mapping in ASP.NET Core.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/exceptions.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
