# Migration: Kck.Pipeline.MediatR → Kck.Pipeline.Mediator

**DiagnosticId:** KCK0200  
**Affects:** `Kck.Pipeline.MediatR` — `KckPipelineBuilder`, `AddKckPipeline()`  
**Status:** Deprecated — will be removed in a future major release

---

## Neden Geçiş?

`Kck.Pipeline.MediatR`, `MediatR 14.x` bağımlılığına sahipti. `MediatR`'ın
lisansı ve ağır bağımlılık ağacı, modern `.NET` kütüphaneleri için ideal
değildir. `Mediator.Abstractions` (alias `Mediator`), sıfır-bağımlılık,
source-generator tabanlı ve AOT uyumlu bir alternatiftir.

---

## Önce / Sonra

### NuGet

```xml
<!-- Önce -->
<PackageReference Include="Kck.Pipeline.MediatR" />

<!-- Sonra -->
<PackageReference Include="Kck.Pipeline.Mediator" />
```

### DI Kaydı

```csharp
// Önce
services.AddKckPipeline(p => p
    .UseMediatR(typeof(Program).Assembly)
    .UseValidationBehavior()
    .UseLoggingBehavior()
    .UseCachingBehavior()
    .UseTransactionBehavior()
    .UseAuthorizationBehavior());

// Sonra
services.AddKckMediator(m => m
    .UseMediator(typeof(Program).Assembly)
    .UseValidationBehavior()
    .UseLoggingBehavior()
    .UseCachingBehavior()
    .UseTransactionBehavior()
    .UseAuthorizationBehavior());
```

### Handler Sınıfı

```csharp
// Önce (MediatR)
using MediatR;

public sealed record GetUserQuery(Guid Id) : IRequest<UserDto>;

public sealed class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public Task<UserDto> Handle(GetUserQuery request, CancellationToken ct) { ... }
}

// Sonra (Mediator)
using Mediator;

public sealed record GetUserQuery(Guid Id) : IRequest<UserDto>;

public sealed class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public ValueTask<UserDto> Handle(GetUserQuery request, CancellationToken ct) { ... }
}
```

**Değişen tek şey:** `using MediatR` → `using Mediator` ve
`Task<T>` → `ValueTask<T>` dönüş tipi.

### Behavior Marker Arayüzleri

`ICachableRequest`, `ISecuredRequest`, `ILoggableRequest`, `ITransactionalRequest`
arayüzleri `Kck.Core.Abstractions.Pipeline` namespace'inde ve **değişmeden** kalır.
İki pakette de aynı arayüzler kullanılır.

### Uyarıyı Susturma (Geçici)

Henüz geçiş yapamayanlar için:

```xml
<!-- .csproj veya Directory.Build.props -->
<NoWarn>$(NoWarn);KCK0200</NoWarn>
```

---

## Referanslar

- [Mediator NuGet](https://www.nuget.org/packages/Mediator.Abstractions)
- [ADR-0021](../adr/0021-deprecate-mediatR-pipeline.md)
