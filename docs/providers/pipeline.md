# Pipeline (MediatR)

MediatR tabanli CQRS/request pipeline + cross-cutting concern behavior'lari.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Pipeline.MediatR` | MediatR + logging / caching / validation / authorization behavior |

## Setup

```csharp
services.AddKckPipelineMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.UseLogging = true;
    cfg.UseValidation = true;
    cfg.UseCaching = true;
    cfg.UseAuthorization = true;
});
```

## Davranis Sirasi

Pipeline asagidaki sirayla calisir:

1. `LoggingBehavior` — her request icin log satiri (request adi + duration)
2. `AuthorizationBehavior` — `IAuthorizationService`'e soruyor
3. `ValidationBehavior` — FluentValidation `IValidator<TRequest>` cagirir
4. `CachingBehavior` — `ICacheable` marker arayuzu implement eden request'leri
   cache'ler
5. Handler calisir

## Cacheable Request

```csharp
public record GetProductQuery(int Id)
    : IRequest<Product>, ICacheable
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
}
```

## Validation

```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
```

Validation hatasi `ValidationException` firlatir — `Kck.Exceptions.AspNetCore`
middleware otomatik 400 ProblemDetails'e cevirir.

## Authorization

```csharp
public record DeleteProductCommand(int Id) : IRequest, IAuthorized
{
    public string RequiredPermission => "products.delete";
}
```

`AuthorizationBehavior` `IPermissionChecker` uzerinden kontrol eder, `false`
donerse `ForbiddenException` firlatir.

## Handler

```csharp
public class GetProductHandler(IRepository<Product> repo)
    : IRequestHandler<GetProductQuery, Product>
{
    public async Task<Product> Handle(GetProductQuery query, CancellationToken ct)
    {
        var product = await repo.FindAsync(query.Id, ct);
        return product ?? throw new NotFoundException($"Product {query.Id}");
    }
}
```

## Dispatch

```csharp
public class ProductsController(IMediator mediator)
{
    [HttpGet("{id}")]
    public Task<Product> Get(int id, CancellationToken ct)
        => mediator.Send(new GetProductQuery(id), ct);
}
```

## Behavior Kapatma

Davranis'i tek request icin atlamak mumkun — `ICacheable` implement etmeyen
request'ler otomatik cache dışı. Authorization icin de ayni — `IAuthorized`
markersız request otomatik gecer.
