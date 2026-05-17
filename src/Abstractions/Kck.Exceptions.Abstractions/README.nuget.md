# Kck.Exceptions.Abstractions

Structured HTTP-aware exception hierarchy — `CustomException` base with concrete types that map automatically to HTTP status codes via `[HttpStatusCode]` attribute.

## Installation

```bash
dotnet add package Kck.Exceptions.Abstractions
```

## Quick Start

```csharp
// Throw typed exceptions in domain/application layer
public async Task<Order> GetOrderAsync(Guid id, CancellationToken ct)
{
    var order = await _repo.GetByIdAsync(id, ct)
        ?? throw new NotFoundException($"Order {id} was not found.");
    return order;
}

public async Task UpdateOrderAsync(UpdateOrderCommand cmd, CancellationToken ct)
{
    if (cmd.Total <= 0)
        throw new BadRequestException("Order total must be greater than zero.");

    if (!await _auth.HasPermissionAsync(cmd.UserId, "orders.write", ct))
        throw new UnauthorizedException("You do not have permission to update orders.");
}

// The exception middleware (Kck.Exceptions.AspNetCore) catches these
// and returns the correct HTTP status code automatically.
```

## Configuration

This is a contracts-only package — no configuration required. Pair with `Kck.Exceptions.AspNetCore` for automatic HTTP mapping in ASP.NET Core.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/exceptions.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
