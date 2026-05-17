# Kck.Pipeline.Abstractions

Pipeline behavior marker interfaces — implement `ICachableRequest`, `ILoggableRequest`, `ISecuredRequest`, or `ITransactionalRequest` on a command/query to opt into cross-cutting behaviors automatically.

## Installation

```bash
dotnet add package Kck.Pipeline.Abstractions
```

## Quick Start

```csharp
// 1. Define a query that opts into caching
public record GetProductQuery(int ProductId)
    : IRequest<Product?>, ICachableRequest
{
    public string CacheKey     => $"product:{ProductId}";
    public TimeSpan? CacheTtl  => TimeSpan.FromMinutes(10);
    public bool BypassCache    => false;
}

// 2. Define a command that requires authorization + transaction
public record PlaceOrderCommand(Guid CustomerId, List<OrderLine> Lines)
    : IRequest<Order>, ISecuredRequest, ITransactionalRequest
{
    public string[] RequiredPermissions => ["orders.create"];
}

// 3. Define a query that should be logged
public record GetAuditLogQuery(Guid EntityId)
    : IRequest<IReadOnlyList<AuditEntry>>, ILoggableRequest;

// The behaviors (registered by bundle packages) intercept the pipeline automatically.
// No handler code changes needed.
```

## Configuration

This is a marker-interfaces package — no configuration required. Pipeline behavior implementations are registered by `Kck.Pipeline.Mediator` or your MediatR pipeline setup.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/pipeline.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
