# Pipeline (Mediator)

CQRS/request pipeline and cross-cutting behaviors built on the source-generator-based
[Mediator](https://github.com/martinothamar/Mediator) (MIT).

## Packages

| Package | Purpose |
|---|---|
| `Kck.Pipeline.Mediator` | Validation, logging, caching, authorization and transaction behaviors |
| `Kck.Pipeline.Abstractions` | Marker interfaces that opt requests into behaviors |

> `Kck.Pipeline.MediatR` was removed in 4.0.0 (MediatR 13+ has a commercial license). Migration:
> [migrations/mediatR-to-mediator.md](../migrations/mediatR-to-mediator.md).

## Setup

```csharp
services.AddMediator(); // dispatcher from the Mediator source generator
services.AddKckMediatorPipeline(p => p
    .UseLoggingBehavior()
    .UseAuthorizationBehavior()
    .UseValidationBehavior()
    .UseCachingBehavior()
    .UseTransactionBehavior());
```

Behaviors run in the order of the `Use…Behavior()` calls.

## Marker interfaces (`Kck.Pipeline.Abstractions`)

| Interface | Behavior | Members |
|---|---|---|
| `ILoggableRequest` | Logging | none (marker) |
| `ISecuredRequest` | Authorization | `string[] Roles`: any one is enough; throws `UnauthorizedException` if there is no user, `ForbiddenException` if no role matches |
| `ICachableRequest` | Caching | `CacheKey`, `BypassCache`, `CacheGroupKey`, `SlidingExpiration` |
| `ITransactionalRequest` | Transaction | none (marker) |

The validation behavior runs all FluentValidation `IValidator<TRequest>` instances registered for the request. If there are
errors, it throws `Kck.Exceptions.ValidationException` (`Kck.Exceptions.AspNetCore` converts this to a 400 ProblemDetails).

## Example

```csharp
public sealed record GetProductQuery(int Id) : IQuery<Product>, ICachableRequest
{
    public string CacheKey => $"product:{Id}";
    public bool BypassCache => false;
    public string? CacheGroupKey => "products";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
}

public sealed class GetProductHandler(IProductRepository repo) : IQueryHandler<GetProductQuery, Product>
{
    public async ValueTask<Product> Handle(GetProductQuery query, CancellationToken ct)
        => await repo.FindAsync(query.Id, ct) ?? throw new NotFoundException($"Product {query.Id}");
}
```

A request that does not implement a marker interface skips the corresponding behavior.
