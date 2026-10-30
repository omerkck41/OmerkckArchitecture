# Frequently Asked Questions

## Setup & Registration

### Which package should I install?

To start a new service, use the template: `dotnet new install Kck.Templates`, then `dotnet new kck-webapi`
([Get started](getting-started/README.md)). To add Kck to an existing app, `Kck.Bundle.WebApi` or
`Kck.Bundle.MinimalApi` registers a set of core providers in one call. If you only need specific modules, install the
individual provider packages (`Kck.Caching.Redis`, `Kck.EventBus.RabbitMq`, and so on).

### "No service for type 'ICacheService' has been registered" error

No caching provider is registered. In `Program.cs`, add one:

```csharp
builder.Services.AddKckCachingInMemory(opt => opt.KeyPrefix = "myapp:");
// or: builder.Services.AddKckCachingRedis(redis => redis.Configuration = "localhost:6379");
```

### Does the order of AddKck* calls matter?

Not for provider registrations: each `AddKck*` call registers its own services. In the Mediator pipeline, the
behaviors run in the order of the `.Use…Behavior()` calls on the `AddKckMediatorPipeline()` builder.

---

## Entity & Persistence

### I can't use a parameterized constructor in my class derived from the abstract Entity<TId>

Every entity base has a `protected` constructor that takes the ID. Call it from the derived class, and keep a private
parameterless constructor for EF Core:

```csharp
using Kck.Core.Abstractions.Entities;

public class Product : FullEntity<Guid>
{
    private Product() { }
    public Product(Guid id, string name) : base(id) { Name = name; }
    public string Name { get; private set; } = string.Empty;
}
```

### Is the soft-delete filter applied automatically?

Repository queries always leave out soft-deleted rows unless `QueryOptions.IncludeDeleted` is set. Your own
`DbContext` queries (`db.Orders...`) are filtered only when you call `modelBuilder.ApplyGlobalSoftDeleteQueryFilter()`
(4.6.0+) at the end of `OnModelCreating`. See [Persistence](providers/persistence.md#global-soft-delete-filter-460).

### Paging with IPaginate<T>

```csharp
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;

public sealed class OrderList(IUnitOfWork uow)
{
    public Task<IPaginate<Order>> PageAsync(int index, CancellationToken ct) =>
        uow.Repository<Order, Guid>().GetListAsync(
            QueryOptions.None,
            predicate: o => o.Total > 0,
            orderBy: q => q.OrderByDescending(o => o.CreatedDate),
            index: index, size: 20, cancellationToken: ct);
}
```

Paging runs a separate `COUNT(*)` query for the total. On large tables, page over a projection with
`Query(...).Select(...).ToPaginateAsync(index, size)` so that only the needed columns are read.

---

## Caching

### Redis "Connection refused" error

`Kck.Caching.Redis` connects in a hosted service when the application starts. If the connection fails, startup
fails with the StackExchange.Redis error.

Checklist:
1. Is the connection string (`RedisCacheOptions.Configuration`) correct for this environment?
2. Is the Redis server reachable? (`redis-cli ping`)
3. With `ASPNETCORE_ENVIRONMENT=Development`, check `appsettings.Development.json`.

### What is the difference between InMemory cache and Redis cache?

| Feature | InMemory | Redis | HybridCache |
|---|---|---|---|
| Distributed | No | Yes | Yes (L2) |
| Speed | Fastest | Network latency | L1 cache hit = InMemory speed |
| Stampede protection | CacheServiceBase (64 stripes) | CacheServiceBase (64 stripes) | Built into HybridCache |
| Multi-instance | No | Yes | Yes |

### What is KeyPrefix used for?

It prevents key collisions when several applications or environments (staging/production)
share the same Redis instance. Recommendation: use the `"{appname}:{env}:"` format.

---

## Event Bus

### My IntegrationEvent was published but the handler did not run

- InMemory: the handler must be registered, usually with `RegisterHandlersFromAssembly(...)` on the
  `AddKckEventBus()` builder, and `Subscribe<TEvent, THandler>()` must not be called for that event (it switches the
  event to handlers resolved by class). A handler exception is logged, not thrown.
- RabbitMQ / Azure Service Bus: call `bus.Subscribe<TEvent, THandler>()` after `Build()` and before `Run()`, and
  register the handler class itself (`services.AddTransient<THandler>()`). See
  [Event Bus](providers/event-bus.md#subscribing-rabbitmq-azure-service-bus).
- Check that the exchange, topic and subscription names match between publisher and consumer.

### Are messages lost if the RabbitMQ connection drops?

The InMemory bus keeps nothing: an event is handled in the publishing process or not at all. The RabbitMQ bus declares
durable queues and acknowledges a message only after its handlers succeed. A publish while the broker is unreachable
fails, so for critical business processes write the event to an outbox table in the same transaction as the data and
publish it from a background job.

---

## Pipeline (Mediator)

### Where is MediatR support?

`Kck.Pipeline.MediatR` was removed in 4.0.0 (ADR-0021, ADR-0028): MediatR 13+ is commercially licensed. Use
`Kck.Pipeline.Mediator` (Mediator, MIT) for the pipeline. Migration guide:
[docs/migrations/mediatR-to-mediator.md](migrations/mediatR-to-mediator.md).

### How do I set the behavior order?

The order of the `.Use…Behavior()` calls on the `AddKckMediatorPipeline()` builder sets the pipeline order:

```csharp
services.AddMediator();
services.AddKckMediatorPipeline(p => p
    .UseLoggingBehavior()       // runs first
    .UseValidationBehavior()    // second
    .UseCachingBehavior());     // third
```

---

## Security

### JWT token "IDX10223: Lifetime validation failed" error

The token has expired, or the clocks of the issuing and validating servers differ. `ITokenService.ValidateTokenAsync`
(`Kck.Security.Jwt`) validates with zero clock skew, so keep server clocks synchronized (NTP). The ASP.NET Core JWT
bearer handler has its own `TokenValidationParameters.ClockSkew` (5 minutes by default).

### Argon2 hash verification is slow

This is expected: Argon2 is deliberately expensive, which slows down password guessing. Tune
`Argon2Options.Iterations` and `MemorySize` for your hardware; the defaults are `Iterations = 3`,
`MemorySize = 65536` (64 MiB).

---

## CI/CD & Build

### NU1004 — lock file package version mismatch

Typical cause: a different SDK version, or a change to `Directory.Packages.props` that was restored for a single
project only.

```bash
dotnet restore --force-evaluate  # at the solution root: regenerates every lock file
dotnet restore --locked-mode     # verifies them, as CI does
```

Use the SDK pinned in `global.json`; lock files contain SDK-coupled packages.

### CS0433 — Azure.Core conflict

It occurs when `Azure.Core` 1.54.0 or later is combined with an older `Azure.Identity` (1.13.x). Upgrade
`Azure.Identity` to 1.21.0 or later in `Directory.Packages.props`.

### "Coverage threshold not met" CI error

Measure coverage locally:

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:TextSummary
cat coverage-report/Summary.txt
```

Thresholds: line 70%, branch 67% (they only go up). Details: [docs/policies/test-coverage.md](policies/test-coverage.md).

---

## Versioning & NuGet

### How is a new version published?

MinVer calculates the version from the latest `v*.*.*` tag; commits after a tag build as a pre-release of the next
patch (for example `4.6.1-alpha.0.3`). Pushing a release tag starts the publish workflow, which waits for maintainer
approval in the `nuget` environment before anything reaches nuget.org (ADR-0027). nuget.org cannot delete a package
(only unlist it), so only maintainers push release tags.

### The package readme does not appear on NuGet.org

Each package ships its own `README.nuget.md` (`PackageReadmeFile` in `Directory.Build.props`). A readme change
appears on nuget.org with the next published version; already published versions keep their readme.
