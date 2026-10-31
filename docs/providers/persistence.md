# Persistence

Repositories and a unit of work over Entity Framework Core 10, plus the parts teams usually write by hand around it:
soft delete that follows the model's delete rules, audit fields, and domain events dispatched after the transaction
commits. New to Kck? Start with [your first Kck API in five minutes](../getting-started/README.md). How this compares
with EF Core alone, ABP and Ardalis.Specification: [Persistence: Kck compared](../persistence-comparison.md).

## Packages

| Package | Contents |
|---|---|
| `Kck.Persistence.Abstractions` | `IRepository<T, TId>` (`IReadRepository` + `IWriteRepository`), `IUnitOfWork`, `QueryOptions` |
| `Kck.Persistence.EntityFramework` | EF Core 10: `EfRepository<T, TId>`, `EfUnitOfWork`, `IEfRepositoryFactory`, interceptors, `ToPaginateAsync` |

## Setup

```csharp
using Kck.Persistence.EntityFramework.Extensions;

services.AddKckPersistence(p => p
    .UseEntityFramework<AppDbContext>(o => o
        .UseSqlServer(builder.Configuration.GetConnectionString("Default")))
    .AddAuditInterceptor());   // CreatedDate / ModifiedDate / soft-delete fields
```

`UseEntityFramework` registers the DbContext, `IUnitOfWork` (scoped) and `IEfRepositoryFactory`. Interceptors registered
with `AddAuditInterceptor()` and `AddDomainEventDispatch<TDispatcher>()` / `AddDomainEventHandlers(...)` are attached to
the DbContext automatically (4.1.0+); an interceptor of the same type already added with `AddInterceptors(...)` inside
`configure` is not added twice (one added in `OnConfiguring` is not seen).

### Domain event handlers (4.6.0+)

```csharp
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Extensions;

public sealed record OrderPlaced(Guid OrderId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed class SendOrderConfirmation(ILogger<SendOrderConfirmation> logger) : IDomainEventHandler<OrderPlaced>
{
    public Task HandleAsync(OrderPlaced domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order {OrderId} placed", domainEvent.OrderId);
        return Task.CompletedTask;
    }
}

services.AddKckPersistence(p => p
    .UseEntityFramework<AppDbContext>(o => o.UseSqlServer(connectionString))
    .AddDomainEventHandlers(typeof(Program).Assembly));
```

`AddDomainEventHandlers` registers every concrete `IDomainEventHandler<TEvent>` in the given assemblies as a scoped
service and dispatches events to them (ADR-0030). A handler receives events of exactly its event type, not derived
types; the order of several handlers for one event is not defined; an event with no handler is not an error. When a
handler fails, the rules under [Interceptors](#interceptors) apply (the data stays saved, the events go back onto the
entity, the next `SaveChanges` dispatches them again), so handlers must be idempotent. It cannot be combined with your
own `IDomainEventDispatcher` registered through `AddDomainEventDispatch<T>()` (`InvalidOperationException`).

The reference app turns a domain event into an integration event for the event bus in
[`OrderPlacedHandler.cs`](https://github.com/omerkck41/OmerkckArchitecture/blob/main/samples/Kck.Sample.Orders/Features/Orders/OrderPlacedHandler.cs);
its test `PlaceOrder_DispatcherFails_EventRetriedOnNextSave` shows the retry, and why a handler can run twice.

### DbContext pooling (4.5.0+)

```csharp
using Kck.Persistence.EntityFramework.Extensions;

services.AddKckPersistence(p => p
    .UseEntityFrameworkPool<AppDbContext>(o => o.UseSqlServer(connectionString), poolSize: 1024)
    .AddAuditInterceptor()
    .AddDomainEventHandlers(typeof(Program).Assembly));
```

`UseEntityFrameworkPool` uses `AddDbContextPool`; the registrations are the same as `UseEntityFramework`. Pooled
contexts share one options instance: `configure` runs once, from the root service provider. The domain event dispatcher
is resolved from a new scope for each dispatch — a handler that saves through a DbContext gets its own scope's context,
not the one that raised the event, and does not see the request's scoped state (tenant, current user, correlation
holders); put what it needs into the event. Pending events of a transaction opened with `Database.BeginTransaction` and
committed directly (below) end with the request, as without pooling: when the context goes back to the pool and is
leased again they are dropped (a warning is logged), never dispatched in another request. Commit transactions through
`IUnitOfWork`.

### DbContext factory

`UseEntityFramework<TContext>` cannot be combined with a singleton `AddDbContextFactory<TContext>` (the default
lifetime) or `AddPooledDbContextFactory<TContext>` for the same context: they register `DbContextOptions<TContext>`
with different lifetimes (per scope and singleton). A factory registered with `ServiceLifetime.Scoped`, or a keyed one,
is fine. From 4.7.0 Kck reports this
with an `InvalidOperationException` that names both calls. With container validation on (ASP.NET Core does it in
Development), a factory registered after `UseEntityFramework` is reported by the container first, as
"Cannot consume scoped service 'DbContextOptions`1[...]' from singleton 'IDbContextFactory`1[...]'".

Instead:

- outside a request (background services, jobs), create a scope with `IServiceScopeFactory` and resolve `IUnitOfWork`
  in it, so audit fields, soft delete and domain events still apply;
- to reuse context instances, use `UseEntityFrameworkPool` (above).

## Using repositories

Repositories come from `IUnitOfWork`:

```csharp
using Kck.Persistence.Abstractions.UnitOfWork;

public sealed class OrderService(IUnitOfWork uow)
{
    public Task<Order?> GetAsync(Guid id, CancellationToken ct) =>
        uow.Repository<Order, Guid>().GetByIdAsync(id, cancellationToken: ct);

    public async Task<Order> CreateAsync(Order order, CancellationToken ct)
    {
        await uow.Repository<Order, Guid>().AddAsync(order, ct);
        await uow.SaveChangesAsync(cancellationToken: ct);
        return order;
    }
}
```

`DeleteAsync` / `DeleteRangeAsync` soft-delete by default (entities implementing `ISoftDeletable`); `permanent: true`
deletes rows. A soft delete follows the same rule as `DbContext.Remove` with the `AuditInterceptor` (below, *What a
soft delete covers*), even when the interceptor is not registered. `RevertSoftDeleteAsync` restores only the row
itself; dependants soft-deleted with it stay deleted.

An entity that does not implement `ISoftDeletable` (a plain `Entity<TId>` or `AuditableEntity<TId>`) is read and
deleted like any other row: the soft-delete filter is not applied, `QueryOptions.IncludeDeleted` has no effect,
`DeleteAsync` deletes the row and `RevertSoftDeleteAsync` returns it unchanged. Before 5.0.0, every read of such an
entity (`GetByIdAsync`, `CountAsync`, `GetListAsync`, `AnyAsync`, `GetAsync`) threw "The LINQ expression ... could not
be translated", because the filter was applied to every entity type. Known limit: a repository of a base type that is
not soft-deletable, in a TPH hierarchy whose subtypes are, applies no soft-delete filter either, so a subtype row that
`DeleteAsync` soft-deleted is still returned by the base-type repository; use a repository of the subtype for such reads.

### What a soft delete covers (4.3.0+)

A soft delete covers the rows a permanent delete would delete: EF's cascade over tracked (loaded) dependants, according
to the model's `DeleteBehavior`. Of those rows:

- soft-deletable ones are soft-deleted, down the whole chain (children of children too);
- rows that are not soft-deletable, owned values and many-to-many links are kept, and so is everything below a kept row
  (even when soft-deletable);
- on an optional relationship (`SetNull` / `ClientSetNull`) the dependant is not deleted; a foreign key set to NULL is
  written back to its original value (for a composite key, only the parts set to NULL).

Principals the entity points to and the other side of a many-to-many relationship are never deleted. A row linked
through a cascading relationship (`Cascade` / `ClientCascade`) to a row deleted permanently in the same `SaveChanges`
(`permanent: true`, or not soft-deletable) is deleted permanently with it, even when soft-deletable; `SetNull` /
`Restrict` relationships do not do that. Known limit: required dependants added (`Added`, not saved yet) to a row
soft-deleted at the same time leave the change tracker through EF's cascade and are not inserted.

Dependants that are not loaded are not touched. On a required `Restrict` relationship, EF refuses the delete when a
dependant is tracked, as it would for a permanent delete. The reference app shows both rules:
[cancelling an order](https://github.com/omerkck41/OmerkckArchitecture/blob/main/samples/Kck.Sample.Orders/Features/Orders/CancelOrder.cs)
soft-deletes its auto-included lines (`Cascade`), and
[deleting a customer](https://github.com/omerkck41/OmerkckArchitecture/blob/main/samples/Kck.Sample.Orders/Features/Customers/CustomerEndpoints.cs)
checks for orders first (`Restrict`), because unloaded orders would not stop it.

## QueryOptions

`QueryOptions` controls soft-deleted rows and change tracking. Read methods take `QueryOptions?`; without it,
`GetAsync`, `GetListAsync` and `GetListByDynamicAsync` track, `AnyAsync`, `GetByIdAsync` and `CountAsync` do not.
Presets: `QueryOptions.None`, `QueryOptions.Tracking`, `QueryOptions.WithDeleted`.

```csharp
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;

public sealed class OrderQueries(IUnitOfWork uow)
{
    private readonly IRepository<Order, Guid> _orders = uow.Repository<Order, Guid>();

    public Task<IPaginate<Order>> LargeOrdersAsync(CancellationToken ct) =>
        _orders.GetListAsync(QueryOptions.None, predicate: o => o.Total > 100, size: 20, cancellationToken: ct);

    public Task<IPaginate<Order>> DeletedOrdersAsync(CancellationToken ct) =>
        _orders.GetListAsync(QueryOptions.WithDeleted, predicate: o => o.IsDeleted, cancellationToken: ct);

    public Task<bool> HasOrdersAsync(string email, CancellationToken ct) =>
        _orders.AnyAsync(predicate: o => o.CustomerEmail == email, cancellationToken: ct);

    // Load a soft-deleted row to change it (tracking must be on):
    public Task<Order?> LoadDeletedForUpdateAsync(Guid id, CancellationToken ct) =>
        _orders.GetAsync(o => o.Id == id, new QueryOptions(IncludeDeleted: true, AsTracking: true), cancellationToken: ct);
}
```

> `QueryOptions.WithDeleted` does not track. For load–change–`SaveChanges`, use
> `new QueryOptions(IncludeDeleted: true, AsTracking: true)`.

### Global soft-delete filter (4.6.0+)

`modelBuilder.ApplyGlobalSoftDeleteQueryFilter()` adds a **named** EF Core query filter that hides soft-deleted rows from
every query of the context, including `db.Orders...` outside repositories:
`ModelBuilderExtensions.SoftDeleteQueryFilterName` (`"KckSoftDelete"`). Repository queries with `IncludeDeleted` lift only
this filter; in your own queries use `IgnoreQueryFilters([ModelBuilderExtensions.SoftDeleteQueryFilterName])`.

```csharp
using Kck.Persistence.EntityFramework.Extensions;

public sealed class ShopDbContext(DbContextOptions<ShopDbContext> options, ITenantProvider tenants) : DbContext(options)
{
    private readonly string _tenantId = tenants.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().HasQueryFilter("Tenant", o => o.TenantId == _tenantId); // must be named
        modelBuilder.ApplyGlobalSoftDeleteQueryFilter();
    }
}
```

- Call `ApplyGlobalSoftDeleteQueryFilter()` **last** in `OnModelCreating`: it covers the entity types in the model at that
  point (types added later, for example with `ApplyConfigurationsFromAssembly`, stay unfiltered).
- EF Core cannot combine unnamed and named filters on one entity type. If a soft-deletable entity type already has an
  unnamed `HasQueryFilter(...)`, `ApplyGlobalSoftDeleteQueryFilter` throws `InvalidOperationException` asking you to
  name it; an unnamed filter added **afterwards** fails at model build with EF Core's own error ("Both anonymous and
  named query filters..."). Name your filter in both cases.
- Inheritance (TPH/TPT/TPC): the filter is added to the root type and applies to its derived types; a soft-deletable
  derived type whose root is not soft-deletable, and a soft-deletable owned type, cannot be filtered: the method
  throws `InvalidOperationException` naming the type. Before 4.7.0 hierarchies failed to build with EF Core's error.
- To keep one entity type out of the global filter:
  `modelBuilder.Entity<Log>().HasQueryFilter(ModelBuilderExtensions.SoftDeleteQueryFilterName, null);`
- `IncludeDeleted` lifts the soft-delete filter for the whole query: soft-deleted children loaded through `includes` or
  reached in the predicate are returned too. Other filters, named or unnamed (tenant), keep applying.
- With the global filter, `DeleteAsync` on a row that is already soft-deleted finds it (before 4.6.0 it threw "not
  found"). Deleting it again keeps its original `DeletedDate` (4.7.0; before, the date was overwritten). An entity
  passed to `DeleteAsync` detached is not trusted: its `DeletedDate` is always stamped.

Before 4.6.0: an unnamed filter on a soft-deletable type (for example a tenant filter) was replaced by the soft-delete
filter without a trace, and `IncludeDeleted` returned no soft-deleted rows when the global filter was applied.

## Paging

`GetListAsync` / `GetListByDynamicAsync` return `IPaginate<T>` (`Items`, `Index`, `Size`, `Count`, `Pages`). In your own
queries use `IQueryable<T>.ToPaginateAsync(index, size)`. A negative `size` or `from`, or a page whose offset
(`(index - from) * size`) exceeds `int.MaxValue`, throws `ArgumentOutOfRangeException` before querying (4.7.0; before,
the offset could overflow into a database error). `ArgumentOutOfRangeException` is not mapped to 400 by the exception
handler, so validate page values that come from a client:

```csharp
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Paging;

public sealed class RecentOrders(IUnitOfWork uow)
{
    public Task<IPaginate<Order>> FirstPageAsync(CancellationToken ct) =>
        uow.Repository<Order, Guid>().Query(QueryOptions.None)   // soft-delete filtered, no tracking
            .OrderByDescending(o => o.CreatedDate)
            .ToPaginateAsync(index: 0, size: 20, cancellationToken: ct);
}
```

## Specifications

A `Specification<T>` keeps a reusable query (criteria, includes, order, paging, `IncludeDeleted()`, `AsTracking()`)
in one class. From 4.8.0 `Kck.Persistence.EntityFramework.Specifications` runs it against any repository:
`ListAsync(spec)`, `FirstOrDefaultAsync(spec)`, `CountAsync(spec)` (criteria only, paging ignored, for a total) and
`WithSpecification(spec)` for the `IQueryable<T>`. The repository's query still applies the soft-delete filter. A
specification has one order key (`ApplyOrderBy` wins over `ApplyOrderByDescending`); when paging, order by a key
that is unique, or rows with equal keys can repeat or be skipped between pages.

```csharp
using Kck.Persistence.Abstractions.Specifications;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Specifications;

public sealed class RecentOrdersOf : Specification<Order>
{
    public RecentOrdersOf(string customerEmail, int page, int size)
    {
        Where(o => o.CustomerEmail == customerEmail);
        ApplyOrderByDescending(o => o.CreatedDate);
        ApplyPaging(skip: page * size, take: size);
    }
}

public sealed class CustomerOrders(IUnitOfWork uow)
{
    public async Task<(List<Order> Items, int Total)> PageAsync(string customerEmail, int page, CancellationToken ct)
    {
        var orders = uow.Repository<Order, Guid>();
        var spec = new RecentOrdersOf(customerEmail, page, size: 20);
        return (await orders.ListAsync(spec, ct), await orders.CountAsync(spec, ct));
    }
}
```

## Dynamic queries

`GetListByDynamicAsync(DynamicQuery)` works only for entity types with an `IFilterPropertyWhitelist<T>` registration; a
filter on a property outside the whitelist throws `ArgumentException`. In the reference app:
[`SearchOrders.cs`](https://github.com/omerkck41/OmerkckArchitecture/blob/main/samples/Kck.Sample.Orders/Features/Orders/SearchOrders.cs)
(whitelist, endpoint, 400 for other fields).

## Transactions

```csharp
using Kck.Persistence.Abstractions.UnitOfWork;

public sealed class PlaceOrder(IUnitOfWork uow)
{
    public async Task RunAsync(Order order, CancellationToken ct)
    {
        await uow.BeginTransactionAsync(ct);
        try
        {
            await uow.Repository<Order, Guid>().AddAsync(order, ct);
            // ... work across several repositories
            await uow.SaveChangesAsync(cancellationToken: ct);
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }

        await uow.CommitAsync(ct); // commits, then dispatches the domain events saved inside the transaction
    }
}
```

The commit is outside the `try`: if dispatching fails after the commit, the data is already durable and `RollbackAsync`
must not be called.

## Interceptors

- `AuditInterceptor` — sets `CreatedDate` / `ModifiedDate` on `IAuditable` entities and turns deletes of
  `ISoftDeletable` entities into `IsDeleted = true` + `DeletedDate` (a row that is already soft-deleted keeps its
  `DeletedDate`). Deletes made through the repository with
  `permanent: true` stay permanent. Soft-deleted rows are hidden only from repository queries and from contexts that
  apply a soft-delete query filter; unique indexes that do not exclude deleted rows can reject re-inserting the same
  key. Tracked (loaded) dependants of a soft-deleted row follow *What a soft delete covers*, even without a navigation.
  In the same `SaveChanges`, a dependant that is not soft-deletable and removed explicitly is kept as well, and a foreign
  key set to NULL explicitly or a many-to-many link removed from a collection is restored — make those changes in a
  separate `SaveChanges`. Out of scope: `ChangeTracker.CascadeDeleteTiming = OnSaveChanges` (the cascade runs after the
  interceptor, so soft-deletable dependants are not soft-deleted).
- `DomainEventDispatchInterceptor` — registered by `AddDomainEventHandlers(...)` or `AddDomainEventDispatch<TDispatcher>()`;
  dispatches aggregates' domain events after `SaveChanges`. Behaviour since 4.2.0:
  - Events saved inside a transaction opened with EF's `BeginTransaction` belong to that transaction; they are dropped on
    rollback, on failure, or when it is disposed without a commit. After the commit, `IUnitOfWork.CommitAsync` /
    `SaveChangesAsync(autoCommitTransaction: true)` dispatches them once the commit has completed; a handler can save
    through the same DbContext. If the transaction was opened with `Database.BeginTransaction` and committed directly,
    the events are dispatched by the context's next `SaveChanges`; if no further `SaveChanges` happens on that context
    they are lost without a trace — commit transactions through `IUnitOfWork`.
  - If the dispatcher fails, the data is saved, the error goes to the caller and the events go back onto the entities;
    the next `SaveChanges` on the same context dispatches them again. Delivery is at least once: a partly successful
    dispatch can repeat, so handlers must be idempotent. If the error ends the request or scope (a web request, a
    Mediator pipeline) or the process crashes, the events are lost — durable delivery needs an outbox.
  - Synchronous `SaveChanges()`: the dispatcher is asynchronous, so dispatch runs on a thread-pool thread and the calling
    thread blocks until it finishes (no deadlock; a warning is logged each time). Use `SaveChangesAsync` in code that
    raises domain events.
  - Out of scope: a direct commit of an external transaction passed with `Database.UseTransaction` (events are dropped),
    `TransactionScope`, rolling back to a savepoint (events raised after the savepoint are still dispatched on commit).
  - DbContext pooling is supported through `UseEntityFrameworkPool` (above); `AddDbContextFactory` is not.
- From 4.7.0 the package's analyzers report a direct commit in a project with domain events
  ([KCK1001](../reference/diagnostics/KCK1001.md)) and a synchronous `SaveChanges`
  ([KCK1002](../reference/diagnostics/KCK1002.md)).

## UTC dates

Audit dates are written in UTC, but SQLite and SQL Server `datetime2` return every `DateTime` with
`DateTimeKind.Unspecified`, so it serializes without the "Z" suffix. From 4.8.0, `UseUtcDateTimes()` reads every
`DateTime` / `DateTime?` back as UTC (and converts a local value to UTC when writing):

```csharp
using Kck.Persistence.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;

public sealed class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.UseUtcDateTimes();
}
```

PostgreSQL `timestamptz` already returns UTC; the convention does no harm there.

## Concurrency

`FullEntity<TId>.RowVersion` is a `[Timestamp]` column, which only SQL Server generates. With SQLite or PostgreSQL the
column is never set, so do not map it there (`Ignore(e => e.RowVersion)`) and rely on another concurrency token (for
PostgreSQL, the `xmin` system column). The `kck-webapi` template does this for its non-SQL Server outputs.

## Migrations

```bash
dotnet ef migrations add InitialCreate --project src/YourApp.Infrastructure
dotnet ef database update
```

The `dotnet-ef` tool is in `.config/dotnet-tools.json`.
