# Migration: Kck.Pipeline.MediatR → Kck.Pipeline.Mediator

**DiagnosticId:** KCK0200  
**Affects:** `Kck.Pipeline.MediatR` — `KckPipelineBuilder`, `AddKckPipeline()`  
**Status:** Deprecated — will be removed in a future major release

---

## Why Migrate?

`Kck.Pipeline.MediatR` depended on `MediatR 14.x`. `MediatR`'s license and
heavy dependency tree are not ideal for modern `.NET` libraries.
`Mediator.Abstractions` (alias `Mediator`) is a zero-dependency,
source-generator-based, AOT-compatible alternative.

---

## Before / After

### NuGet

```xml
<!-- Before -->
<PackageReference Include="Kck.Pipeline.MediatR" />

<!-- After -->
<PackageReference Include="Kck.Pipeline.Mediator" />
```

### DI Registration

```csharp
// Before
services.AddKckPipeline(p => p
    .UseMediatR(typeof(Program).Assembly)
    .UseValidationBehavior()
    .UseLoggingBehavior()
    .UseCachingBehavior()
    .UseTransactionBehavior()
    .UseAuthorizationBehavior());

// After
services.AddKckMediator(m => m
    .UseMediator(typeof(Program).Assembly)
    .UseValidationBehavior()
    .UseLoggingBehavior()
    .UseCachingBehavior()
    .UseTransactionBehavior()
    .UseAuthorizationBehavior());
```

### Handler Class

```csharp
// Before (MediatR)
using MediatR;

public sealed record GetUserQuery(Guid Id) : IRequest<UserDto>;

public sealed class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public Task<UserDto> Handle(GetUserQuery request, CancellationToken ct) { ... }
}

// After (Mediator)
using Mediator;

public sealed record GetUserQuery(Guid Id) : IRequest<UserDto>;

public sealed class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public ValueTask<UserDto> Handle(GetUserQuery request, CancellationToken ct) { ... }
}
```

**The only changes:** `using MediatR` → `using Mediator` and the
return type `Task<T>` → `ValueTask<T>`.

### Behavior Marker Interfaces

The `ICachableRequest`, `ISecuredRequest`, `ILoggableRequest` and
`ITransactionalRequest` interfaces live in the `Kck.Core.Abstractions.Pipeline`
namespace and stay **unchanged**. Both packages use the same interfaces.

### Suppressing the Warning (Temporary)

If you cannot migrate yet:

```xml
<!-- .csproj or Directory.Build.props -->
<NoWarn>$(NoWarn);KCK0200</NoWarn>
```

---

## References

- [Mediator NuGet](https://www.nuget.org/packages/Mediator.Abstractions)
- [ADR-0021](../adr/0021-deprecate-mediatR-pipeline.md)
