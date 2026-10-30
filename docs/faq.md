# Frequently Asked Questions

## Setup & Registration

### Which package should I install?

For a quick start, add `Kck.Bundle.WebApi` or `Kck.Bundle.MinimalApi`. A single package brings
all the core providers. If you only need specific modules, prefer the individual provider
packages (`Kck.Caching.Redis`, `Kck.EventBus.RabbitMq`, and so on).

### "No service for type 'ICacheService' has been registered" error

You skipped `AddKckCaching()` or the caching section of the bundle. In `Program.cs`:

```csharp
builder.Services.AddKckCaching(opts => opts.KeyPrefix = "myapp:");
builder.Services.AddKckInMemoryCache();  // or AddKckRedisCache(...)
```

### Does the order of AddKck* calls matter?

Register the abstraction first, then the provider: `AddKckCaching()` → `AddKckRedisCache()`. In the
Mediator pipeline, however, watch the order of behaviors: `AddKckMediatorPipeline()` runs the
behaviors in the order of the `.Use…Behavior()` calls on the builder.

---

## Entity & Persistence

### I can't use a parameterized constructor in my class derived from the abstract Entity<TId>

Entity has a `protected Entity(TId id)` constructor. Call it in the derived class:

```csharp
public class Product : Entity<Guid>
{
    private Product() { }
    public Product(Guid id, string name) : base(id) { Name = name; }
    public string Name { get; private set; } = string.Empty;
}
```

### "The entity type requires a primary key" error in an EF Core migration

EF Core does not automatically recognize `Entity<TId>.Id` as the primary key. Specify it in
`OnModelCreating` or in a fluent configuration:

```csharp
modelBuilder.Entity<Product>().HasKey(p => p.Id);
```

We recommend configuring it in a separate file with `IEntityTypeConfiguration<T>`.

### Is the SoftDelete filter applied automatically?

No. `EfRepository` does not add a global filter to its default queries. You must add
`HasQueryFilter(e => !e.IsDeleted)` to your `DbContext` yourself. This is a deliberate design
decision: screens that show deleted records, such as an admin panel, may not need the filter.

### Using IQueryable with Paginate<T>

```csharp
// CORRECT — Where/OrderBy first, then ToPaginateAsync
var result = await _repo.GetListAsync(
    predicate: p => p.IsActive,
    orderBy: q => q.OrderBy(p => p.Name),
    index: 0, size: 20, cancellationToken: ct);
```

The `ToPaginateAsync` extension method runs a separate `COUNT(*)` query for `totalCount`.
On large tables, use `QueryOptions` to `Select` only the fields you need for better performance.

---

## Caching

### Redis "Connection refused" error

`RedisConnectionHostedService` connects when the application starts. If the connection fails,
the application still starts, but `ICacheService` calls throw `InvalidOperationException`.

Checklist:
1. Is `CacheOptions.RedisConnectionString` correct?
2. Is the Redis server reachable? (`redis-cli ping`)
3. If `ASPNETCORE_ENVIRONMENT=Development`, check `appsettings.Development.json`.

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

1. Did you call `Subscribe<TEvent, THandler>()`? The DI container registers the handler, but
   you must also subscribe separately (usually at startup).
2. Is the handler registered in DI? Register handlers through the `AddKckEventBus()` builder.
3. If you use RabbitMQ/Azure, verify that the exchange/queue names match.

### Are messages lost if the RabbitMQ connection drops?

`InMemoryEventBus` is fire-and-forget: there is no persistence. If you use `RabbitMqEventBus`,
make sure you define the exchange and queue with `durable: true` for durability.
For critical business processes, consider integrating the Outbox pattern (see: Phase E list).

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

There is a difference between the server clock and the token creation time. Check the `ClockSkew` tolerance:

```csharp
options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(5);
```

### Argon2 hash verification is slow

This is expected. Argon2 is deliberately slow (a design that counters DoS attacks).
Adjust the `Argon2Options.Iterations` and `MemorySize` values for your environment.
Minimum recommended: `Iterations=3, MemorySize=65536`.

---

## CI/CD & Build

### NU1004 — lock file package version mismatch

Typical cause: a different SDK version or a change to Directory.Packages.props.

```bash
dotnet restore --force-evaluate  # regenerates the lock files
git add "**packages.lock.json"
```

Pin the SDK version (global.json). Details: [ADR-0011](adr/0011-multi-target-net8-net10.md).

### CS0433 — Azure.Core conflict

It can occur when `Azure.Identity >= 1.21.0` and `Azure.Core >= 1.54.0` are used together.
Upgrade to `Azure.Identity 1.21.0+` in `Directory.Packages.props`. Details:
[MEMORY: Azure CS0433](https://github.com/omerkck41/OmerkckArchitecture/blob/main/SECURITY.md).

### "Coverage threshold not met" CI error

Measure coverage locally:

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:TextSummary
cat coverage-report/Summary.txt
```

Thresholds: `Line ≥50%, Branch ≥45%`. Details: [docs/policies/test-coverage.md](policies/test-coverage.md).

---

## Versioning & NuGet

### I forgot to tag the new version and NuGet shows the old version

MinVer calculates the version from the tag. If there is no tag, it adds a `-preview.N` suffix. To publish:

```bash
git tag v2.1.0
git push origin v2.1.0   # CI pushes to NuGet automatically
```

### The package readme does not appear on NuGet.org

`<PackageReadmeFile>README.md</PackageReadmeFile>` must be defined in `Directory.Build.props`
(added as of Phase C). Older versions must be republished.
