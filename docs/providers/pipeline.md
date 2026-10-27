# Pipeline (Mediator)

Source-generator tabanli [Mediator](https://github.com/martinothamar/Mediator) (MIT) uzerinde CQRS/request pipeline ve
cross-cutting behavior'lar.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Pipeline.Mediator` | Validation, logging, caching, authorization, transaction behavior'lari |
| `Kck.Pipeline.Abstractions` | Request'lerin behavior'a katilmasini saglayan marker arayuzler |

> `Kck.Pipeline.MediatR` 4.0.0'da kaldirildi (MediatR 13+ ticari lisans). Gecis:
> [migrations/mediatR-to-mediator.md](../migrations/mediatR-to-mediator.md).

## Setup

```csharp
services.AddMediator(); // Mediator source generator'in dispatcher'i
services.AddKckMediatorPipeline(p => p
    .UseLoggingBehavior()
    .UseAuthorizationBehavior()
    .UseValidationBehavior()
    .UseCachingBehavior()
    .UseTransactionBehavior());
```

Behavior'lar `Use…Behavior()` cagrilarinin sirasiyla calisir.

## Marker arayuzler (`Kck.Pipeline.Abstractions`)

| Arayuz | Behavior | Uyeler |
|---|---|---|
| `ILoggableRequest` | Logging | yok (isaret) |
| `ISecuredRequest` | Authorization | `string[] Roles` — biri yeterli; kullanici yoksa `UnauthorizedException`, rol yoksa `ForbiddenException` |
| `ICachableRequest` | Caching | `CacheKey`, `BypassCache`, `CacheGroupKey`, `SlidingExpiration` |
| `ITransactionalRequest` | Transaction | yok (isaret) |

Validation behavior'i request icin kayitli tum FluentValidation `IValidator<TRequest>`'lari calistirir; hata varsa
`Kck.Exceptions.ValidationException` firlatir (`Kck.Exceptions.AspNetCore` bunu 400 ProblemDetails'e cevirir).

## Ornek

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

Marker arayuzu uygulamayan request ilgili behavior'i atlar.
