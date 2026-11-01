# Kck.Pipeline.Abstractions

Pipeline behavior marker interfaces — implement `ICachableRequest`, `ILoggableRequest`, `ISecuredRequest`, or `ITransactionalRequest` on a command/query to opt into cross-cutting behaviors automatically.

## Installation

```bash
dotnet add package Kck.Pipeline.Abstractions
```

## Quick Start

```csharp
using Kck.Pipeline.Abstractions;
using Mediator;

// 1. A query that opts into caching
public sealed record GetProductQuery(int ProductId) : IQuery<Product?>, ICachableRequest
{
    public string CacheKey => $"product:{ProductId}";
    public bool BypassCache => false;
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
}

// 2. A command that requires a role and runs in a transaction
public sealed record PlaceOrderCommand(Guid CustomerId, IReadOnlyList<Guid> ProductIds)
    : ICommand<Guid>, ISecuredRequest, ITransactionalRequest
{
    public string[] Roles => ["customer"];
}

// 3. A command that is logged
public sealed record CancelOrderCommand(Guid OrderId) : ICommand, ILoggableRequest;

// The Kck.Pipeline.Mediator behaviors (AddKckMediatorPipeline) act on these markers; requests without a marker skip
// the matching behavior.
```

## Configuration

This is a marker-interfaces package — no configuration required. Pipeline behavior implementations are registered by `Kck.Pipeline.Mediator`.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/pipeline.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
