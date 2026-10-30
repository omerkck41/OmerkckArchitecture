# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `Kck.Http.Abstractions` — `ResilienceOptions.CircuitBreakerFailureRatio` (default 0.1) and
  `CircuitBreakerMinimumThroughput` (default 100), applied to the standard resilience handler's circuit breaker.
- `Kck.BackgroundJobs.Hangfire` — `HangfireOptions.ConfigureStorage`: plug in a durable Hangfire storage (for example
  `c => c.UseSqlServerStorage(...)` from Hangfire.SqlServer) instead of the in-memory one.

### Deprecated
- `ResilienceOptions.CircuitBreakerFailureThreshold` (KCK0900) — never applied; use the two options above.
- `HangfireOptions.ConnectionString` (KCK0700) — never read; use `ConfigureStorage`.
- `RsaKeySource.SecretsManager` (KCK0300) — never implemented; set `RsaKeyBase64` with the key you read yourself.
- Bundle settings that were never read (KCK0002): `KckWebApiDefaults.Persistence`, `.Caching`, `.Observability`,
  `SecurityDefaults.Hashing`, `JwtDefaults.Algorithm` (WebApi and MinimalApi), `KckWorkerServiceDefaults.Observability`.
  Configure those providers in code. Each of these deprecations is removed in v5.0 at the earliest.

### Fixed
- `Kck.EventBus.Abstractions` (RabbitMQ, Azure Service Bus) — a handler subscribed with `Subscribe<TEvent, THandler>()`
  but registered through `RegisterHandlersFromAssembly` (as `IEventHandler<TEvent>`) was not found, and its messages
  were skipped with only a log entry. `RegisterHandlersFromAssembly` now also registers each handler class, and a
  subscribed handler is otherwise looked up among the `IEventHandler<TEvent>` registrations.
- `Kck.Persistence.EntityFramework` — `ApplyGlobalSoftDeleteQueryFilter()` failed to build models with an inheritance
  hierarchy (TPH, TPT, TPC). The filter is now added to root types only; derived types inherit it. A soft-deletable
  owned type, or a soft-deletable derived type whose root is not soft-deletable, now fails with an
  `InvalidOperationException` that names the type (no filter could hide its deleted rows) instead of EF Core's
  configuration error.
- `Kck.Persistence.EntityFramework` — `ToPaginateAsync` (and the repository's paged reads) computed the offset
  `(index - from) * size` unchecked, so a large client-supplied page index became a negative offset and a database
  error. A negative `size` or `from`, or an offset beyond `int.MaxValue`, now throws `ArgumentOutOfRangeException`
  before querying (`index < from` still throws `ArgumentException`). Validate client-supplied page values: the
  exception handler maps both to 500.

### Changed — behaviour
- `Kck.EventBus.InMemory` — `PublishAsync` runs every registered `IEventHandler<TEvent>` plus each subscribed handler
  not among them, once. Before, a single `Subscribe<TEvent, THandler>()` call replaced the registered handlers for that
  event, and a subscribed handler registered only as `IEventHandler<TEvent>` was skipped without a log entry.
- `Kck.Persistence.EntityFramework` — deleting a row that is already soft-deleted (repository `DeleteAsync` or
  `DbContext.Remove` with the audit interceptor) keeps its `DeletedDate` instead of overwriting it. An entity passed to
  `DeleteAsync` detached is not trusted: its `DeletedDate` is always stamped.
- `Kck.EventBus.Abstractions` — `RegisterHandlersFromAssembly` registers each handler once (`TryAddEnumerable`), so
  scanning an assembly twice no longer runs its handlers twice.
- `Kck.Http.Resilience` — `CircuitBreakerDuration` is now the circuit breaker's break duration (how long the circuit
  stays open). Before, it set the sampling window (2 x the value) and the break duration stayed at the handler's 5 s
  default. The sampling window is now 2 x `Timeout` with a 30 s floor, so a short break with a long timeout is valid.
- `Kck.Security.Jwt` — `RsaKeySource.SecretsManager` fails options validation at startup instead of throwing at the
  first token.
- `Kck.Localization.Json` / `.Yaml` — `LocalizationOptions.CacheExpiration` (default 30 minutes) is applied: a cached
  resource file is read again after it expires. Before, it was cached until `ReloadAsync`.
- `Kck.Exceptions.AspNetCore` — each handled exception is logged once: `Warning` for a 4xx, `Error` for a 5xx
  (including a `CustomException` without a status code, logged at `Warning` before). Before, the middleware also
  logged every exception at `Error`.

### Changed
- `schemas/appsettings.kck.schema.json` lists only the keys the bundles read (`Kck:Security:Jwt:Issuer`/`Audience`,
  `Kck:AspNetCore:*`, `Kck:BackgroundJobs:Provider`). The removed sections (`Caching`, `EventBus`, `Messaging`,
  `Security:Argon2`) were never read; providers are configured in code.

## [4.6.0] — 2026-10-30

### Added
- `Kck.Core.Abstractions` — `IDomainEventHandler<TEvent>`: handles one domain event type (ADR-0030).
- `Kck.Persistence.EntityFramework` — `AddDomainEventHandlers(params Assembly[])`: registers every concrete
  `IDomainEventHandler<TEvent>` in the assemblies (scoped) and dispatches domain events to them, so no custom
  `IDomainEventDispatcher` is needed. A handler receives events of exactly its type; delivery keeps the existing
  semantics (after commit, at least once). Combining it with `AddDomainEventDispatch<T>()` throws
  `InvalidOperationException`.
- `Kck.Templates` — `kck-webapi` options `--database sqlite|postgres|sqlserver`, `--include-tests`, `--include-sample`.

### Changed — behaviour
- `Kck.Templates` — `kck-webapi` generates a solution (`.slnx`) with `src/<Name>.Api` and `tests/<Name>.Api.Tests`
  instead of a single controller project: minimal APIs, EF Core persistence on SQLite (soft delete, audit fields,
  domain events after commit), an order management sample, JWT bearer authentication (`dotnet user-jwts` for local
  tokens), health checks, OpenAPI with the Scalar UI in development, and integration tests. Option names are now
  lower-case (`--include-sample`). Projects generated earlier are not affected; the previous output is
  `dotnet new install Kck.Templates::4.5.0`.

Applies to contexts that call `ApplyGlobalSoftDeleteQueryFilter()`; others behave as before:
- `Kck.Persistence.EntityFramework` — `ApplyGlobalSoftDeleteQueryFilter()` throws `InvalidOperationException` when a
  soft-deletable entity type already has an unnamed query filter, asking you to name it. Before, it replaced that filter
  (for example a tenant filter) without a trace. An unnamed filter added *after* the call now fails at model build with
  EF Core's own error; name it too. Named filters now work next to it. To keep one entity out of the global filter:
  `HasQueryFilter(ModelBuilderExtensions.SoftDeleteQueryFilterName, null)`.
- `QueryOptions.IncludeDeleted` lifts the soft-delete filter for the whole query: soft-deleted children loaded with
  `includes` or reached in the predicate are returned too (before, none were — the filter was not lifted at all).
- `DeleteAsync` on a row that is already soft-deleted now finds it and soft-deletes it again (`DeletedDate` is updated),
  as contexts without the global filter already did; before, it threw "not found".

### Fixed
- `Kck.Persistence.EntityFramework` — with `ApplyGlobalSoftDeleteQueryFilter()`, repository reads with
  `QueryOptions.IncludeDeleted` (`QueryOptions.WithDeleted`, `RevertSoftDeleteAsync`) found no soft-deleted rows: the
  repository lifted only its own filter, not the model's. The global filter is now an EF Core named filter
  (`ModelBuilderExtensions.SoftDeleteQueryFilterName`, `"KckSoftDelete"`) and `IncludeDeleted` lifts only that one;
  other named filters (tenant) keep applying.

## [4.5.0] — 2026-10-30

### Added
- Native AOT: `Kck.Http.Resilience` is `IsAotCompatible`, verified by the CI native AOT smoke (typed HTTP client round
  trip). `Kck.Observability.OpenTelemetry` is not: `OpenTelemetry.Instrumentation.EntityFrameworkCore` (beta) produces
  trim warnings (IL2104).
- `Kck.Http.Abstractions` — `ResilienceOptions.SerializerOptions`: JSON options for request bodies and responses of
  `ResilientApiClient`, e.g. a source-generated `JsonSerializerContext` for Native AOT. `null` keeps the web defaults
  (camelCase, reflection), as before.
- `Kck.Persistence.EntityFramework` — `UseEntityFrameworkPool<TContext>(configure, poolSize)`: DbContext pooling with
  the same registrations as `UseEntityFramework`. `AddAuditInterceptor` and `AddDomainEventDispatch` work with it; the
  pooled domain event interceptor resolves the dispatcher from a new scope for each dispatch, and events a lease left
  waiting are dropped when the context is leased again (never dispatched in another request).

### Changed
- `Kck.Observability.OpenTelemetry` — the health endpoints write their JSON with source-generated metadata; the body
  is unchanged (`status`, `checks[name, status, description, duration]`, `totalDuration`).

## [4.4.0] — 2026-10-30

### Added
- Native AOT: `Kck.Security.Jwt`, `Kck.Localization`, `Kck.Localization.Json` and `Kck.Resilience.Polly` are marked
  `IsAotCompatible` too; the CI native AOT smoke now issues and validates a JWT, reads a JSON resource and runs a retry
  pipeline. `Kck.Logging.Serilog` is not: Serilog itself produces trim warnings (IL2104) under Native AOT.

### Changed — behaviour
- `Kck.AspNetCore` — `ApiResponseHelper.CreateSuccess` on a GET with `null` data now returns `Success = false` (see
  Fixed). Code that read `Success` to detect "not found" there now sees `false`; check `StatusCode == 404` or call
  `CreateFail` directly.

### Fixed
- `Kck.Pipeline.Mediator` — `TransactionBehavior`: when the handler failed and the rollback failed too, the rollback
  error replaced the handler's. The handler's error is rethrown; the rollback error is logged (`Error`).
- `Kck.AspNetCore` — `ApiResponseHelper.CreateSuccess` returned `Success = true` with status 404 for a GET with
  `null` data. That case is now the same 404 failure as `CreateFail` (`Success = false`, `TraceId` set; the message
  is kept).

## [4.3.0] — 2026-10-29

### Added
- Native AOT: `Kck.Pipeline.Mediator`, `Kck.Caching.InMemory`, `Kck.EventBus.InMemory`, `Kck.FeatureFlags.InMemory`,
  `Kck.Security.Argon2` and `Kck.Security.Totp` are marked `IsAotCompatible` (trim/AOT analyzers on, 0 warnings).
  CI publishes and runs `samples/Kck.AotSmoke` with `PublishAot` on every build.
- `Kck.Pipeline.Mediator` — `UseCachingBehavior(Action<CachingBehaviorOptions>)`: `CachingBehaviorOptions.SerializerOptions`
  takes JSON options with a source-generated `JsonSerializerContext` for cached responses (required under Native AOT;
  without it the behavior throws `InvalidOperationException` on the first cacheable request). Default behaviour
  (reflection, `JsonSerializerOptions.Default` settings) is unchanged.

### Changed — behaviour
- Repository soft delete (`DeleteAsync` / `DeleteRangeAsync` without `permanent`):
  - optional soft-deletable dependents (`SetNull` / `ClientSetNull`) are no longer soft-deleted; they stay linked to
    the soft-deleted owner. Configure `DeleteBehavior.Cascade` on relationships whose dependents should go with it;
  - soft-deletable dependents of dependents are soft-deleted too (before: one level only);
  - soft-deleting an entity that was added but never saved detaches it instead of failing on `SaveChanges`.
- `AuditInterceptor`: soft-deletable rows below a kept, non-soft-deletable dependent (e.g. notes of an order line
  when the order is soft-deleted) are no longer soft-deleted; the line and everything below it stay. A dependent kept
  only to restore its foreign key is no longer stamped with `ModifiedDate`.
- `RevertSoftDeleteAsync` restores only the entity itself (documented; unchanged).

### Deprecated
- `Kck.Persistence.Abstractions` — `IQuery<T>.Query(filter, orderBy, withDeleted, enableTracking)` (and the same
  `EfRepository` method) is obsolete (`KCK0101`), removed in v5.0. Use the new `IQuery<T>.Query(QueryOptions options,
  filter, orderBy)` member: `repo.Query()` → `repo.Query(QueryOptions.None)`, `Query(withDeleted: true)` →
  `Query(QueryOptions.WithDeleted)`, `Query(enableTracking: true)` → `Query(QueryOptions.Tracking)`. The new member
  has a default implementation, so existing `IQuery<T>` implementations keep compiling.
  - `repo.Query(default)` is now ambiguous (CS0121); write `repo.Query(QueryOptions.None)`.
  - Mocks (NSubstitute / Moq) set up on the old overload are not hit by calls to the new one; move the setups.

### Fixed
- `Kck.Persistence.Abstractions` — the parameterized constructors of `Filter` and `Sort` could not be called
  (`CS9035`: they set the `required` members but were not marked `[SetsRequiredMembers]`); now they can.
- `FilterValidator.Validate` compared `isnull` / `isnotnull` case-sensitively while every other operator is
  case-insensitive: `"IsNull"` with a null value was rejected and `"IsNull"` with a value was accepted.
- `Kck.Persistence.EntityFramework` — repository soft delete and `DbContext.Remove` under `AuditInterceptor` now
  follow one rule; before, they gave different rows for the same delete. A soft delete covers the rows a hard delete
  would remove (EF's cascade on tracked dependents, by `DeleteBehavior`): soft-deletable rows are soft-deleted down
  the whole chain, the others are kept together with everything below them, and optional foreign keys set to `null`
  by the cascade are restored. Rows that a hard delete in the same `SaveChanges` cascades to (`Cascade` /
  `ClientCascade`) are deleted with it. Repository soft delete no longer needs `AuditInterceptor` for this, and also
  covers the loaded dependents of a detached entity.
- `AuditInterceptor` — a composite optional foreign key whose nullable part the cascade set to `null` (e.g.
  `TenantId` kept, `CartId` nulled) lost its link to the soft-deleted owner; the nulled parts are now restored.
- `Kck.Persistence.EntityFramework` — `EfUnitOfWork.CommitAsync` (and `SaveChangesAsync(autoCommitTransaction: true)`)
  passed the caller's token to the database commit: a request aborted while the commit ran left its outcome unknown
  (the data could be durable while the caller saw a cancellation). The token is now checked before the commit starts
  (canceled → rollback and `OperationCanceledException`); the commit itself runs to completion. Domain event dispatch
  after the commit still observes the token: canceled there, the data is durable and the caller sees the
  cancellation; the events go back on their entities (as for a failing dispatcher).
- `EfUnitOfWork` — when the commit failed and disposing the transaction also threw, the dispose error hid the commit
  error. The commit error is rethrown.
- `Kck.Pipeline.Mediator` — `TransactionBehavior` rolled back with the request's token; for a canceled request the
  rollback was canceled too and the transaction was only ended by its disposal. It now rolls back with
  `CancellationToken.None`.
- Documentation — README quick starts that did not compile: root README (`AddKckWebApi`, `AddKckExceptions`, `UseKckExceptionHandler`
  do not exist → `AddKckWebApiDefaults` / `UseKckWebApiDefaults`, `AddKckExceptionHandling` /
  `UseKckExceptionHandling`), `Kck.Bundle.MinimalApi` (`UseKckMinimalApiDefaults` does not exist),
  `Kck.Persistence.EntityFramework` (`AddKckEntityFramework` and `IRepository<T>` do not exist; `SaveChangesAsync(ct)`
  bound the token to `autoCommitTransaction`), `Kck.Pipeline.Mediator` (`AddKckMediator` options and the performance
  behaviour do not exist), `Kck.Caching.InMemory` (undefined `_db`). These README blocks and those of the three bundles
  and `Kck.Persistence.Abstractions` are now compiled in CI (`samples/Kck.ReadmeSnippets`).

## [4.2.5] — 2026-10-29

### Fixed
- `Kck.Persistence.EntityFramework` — repository soft delete (`DeleteAsync` / `DeleteRangeAsync` without `permanent`)
  also soft-deleted every loaded soft-deletable entity the deleted entity pointed at: the principal it references
  (which other rows may share) and the other side of a many-to-many (e.g. shared tags). It now cascades only to the
  entity's dependents, as its internal helper described.

### Changed — behaviour
- A loaded principal referenced by a soft-deleted entity stays active. Code that relied on the old cascade must
  delete the principal itself. This includes one-to-one relationships whose foreign key sits on the deleted entity
  (e.g. `Order.ShippingAddressId`): the referenced row is no longer soft-deleted — delete it explicitly, or put the
  foreign key on the other side.

## [4.2.4] — 2026-10-29

### Fixed
- `Kck.Persistence.EntityFramework` — `AuditInterceptor`: `DbContext.Remove` on a soft-deletable entity kept the entity
  (soft delete) but EF's cascade still hit its tracked dependents: required ones that are not soft-deletable were
  hard-deleted (with or without a navigation), owned values (table splitting) were set to `NULL`, many-to-many links
  were deleted, and optional dependents had their foreign key set to `NULL`. These dependents now stay linked to the
  kept row; soft-deletable dependents are soft-deleted as before. Repository deletes with `permanent: true` and
  entities that are not soft-deletable still cascade.
  - In the same `SaveChanges` as its soft-deleted owner, a non-soft-deletable dependent removed explicitly is also
    kept, and a foreign key set to `null` or a many-to-many link removed explicitly is restored; make those changes in
    a separate `SaveChanges`.
  - Not covered: `ChangeTracker.CascadeDeleteTiming = OnSaveChanges`.

## [4.2.3] — 2026-10-29

### Fixed
- `Kck.Persistence.EntityFramework` — `DomainEventDispatchInterceptor` never dispatched the domain events of entities
  that were deleted (`DbContext.Remove` on an entity that is not soft-deletable, or a repository delete with
  `permanent: true`): it collected events after the save, when EF had already detached deleted entities. Events are
  now collected before the save (and again after it, for events raised during the save); if the save fails, is
  canceled, hits a concurrency conflict or is rejected by a later interceptor, they go back on their entities.
  Across entities, events now follow the change tracker's order before the save (added entities first); the order
  of one entity's events is unchanged.

## [4.2.2] — 2026-10-28

### Fixed
- `Kck.Persistence.EntityFramework` — a failed database commit left `EfUnitOfWork` holding the broken transaction.
  `Kck.Pipeline.Mediator`'s `TransactionBehavior` never rolls back after `CommitAsync` (it must not after a commit
  that succeeded), so every later `ITransactionalRequest` in the same scope reused that transaction and failed.

### Changed — behaviour
- `EfUnitOfWork.CommitAsync` now attempts a rollback and releases the transaction itself when the database commit
  fails, then rethrows the commit error. Rule: once `CommitAsync` returns or throws, the transaction has ended. This
  replaces the 4.2.1 note that a failed commit keeps the transaction for the caller.
  - Migration: move `CommitAsync` out of a `try { … } catch { await RollbackAsync(); throw; }` block (see the
    transaction example in `docs/providers/persistence.md`); otherwise the catch now throws "No active transaction"
    and hides the commit error.
  - When the commit outcome is unknown (connection lost or cancellation during the commit), the changes may already be
    durable while the transaction's domain events are dropped.
  - Custom `IUnitOfWork` implementations used with `TransactionBehavior` should follow the same rule.
- `IUnitOfWork` documents the transaction lifecycle and the `InvalidOperationException` it throws.

## [4.2.1] — 2026-10-28

### Fixed
- `Kck.Persistence.EntityFramework` — `EfUnitOfWork` kept its transaction after `CommitAsync`, `RollbackAsync` and
  `SaveChangesAsync(autoCommitTransaction: true)`, so a second `BeginTransactionAsync` on the same unit of work started
  nothing: later saves ran outside a transaction, their domain events were dispatched at once, and `CommitAsync` failed
  on the finished transaction. The transaction is now released after a successful commit and after every rollback
  (a failed commit keeps it so the caller can roll back; `RollbackAsync` releases it even when the rollback itself
  fails). `CommitAsync` / `RollbackAsync` called again after a commit or rollback now throw
  `InvalidOperationException` ("No active transaction") instead of a provider error.

## [4.2.0] — 2026-10-28

Changes when and how domain events are dispatched for apps that call `AddDomainEventDispatch<TDispatcher>()`; read
"Changed — behaviour" before upgrading.

### Fixed
- `Kck.Persistence.EntityFramework` — `DomainEventDispatchInterceptor`:
  - Events saved inside an EF transaction are no longer dispatched before the commit: a rolled-back, failed or
    uncommitted-and-disposed transaction drops them, so a change that never became durable no longer sends e-mails
    or messages.
  - If the dispatcher throws, the events are put back on their entities and the next `SaveChanges` dispatches them
    again; before, they were cleared first and lost. The exception still reaches the caller after the data is saved.
    Delivery is at-least-once: handlers should be idempotent.

### Changed — behaviour
- Events saved inside a transaction are dispatched after the commit, outside the transaction:
  `IUnitOfWork.CommitAsync` and `SaveChangesAsync(autoCommitTransaction: true)` dispatch them once the commit has
  returned; for a transaction committed directly through `Database.BeginTransaction`, the context's next
  `SaveChanges` dispatches them — if none follows, they are lost, so commit through `IUnitOfWork`. Handlers that
  relied on writing inside the caller's transaction now write after it (consider an outbox for atomic delivery).
  Events put back after a dispatcher failure are retried only by a later save on the same context.
- Do not call `RollbackAsync` when `CommitAsync` throws: the data is committed; see the transaction example in
  `docs/providers/persistence.md`. `Kck.Pipeline.Mediator`'s `TransactionBehavior` now commits outside its
  rollback block for the same reason.
- Not covered: external transactions passed with `Database.UseTransaction` and committed directly (events are
  dropped), `TransactionScope`, and rolling back to a savepoint.
- `Kck.Persistence.EntityFramework` now depends on `Microsoft.EntityFrameworkCore.Relational` (same version as EF
  Core) for the transaction interceptor. Relational providers already reference it.
  Decision: [decisions/2026-10-28-efcore-relational-dependency.md](decisions/2026-10-28-efcore-relational-dependency.md).

## [4.1.0] — 2026-10-28

The fixes below change runtime behaviour for apps that call `AddAuditInterceptor()` or
`AddDomainEventDispatch<TDispatcher>()`; read "Changed — behaviour" before upgrading.

### Fixed
- `Kck.Persistence.EntityFramework`: `AddAuditInterceptor()` and `AddDomainEventDispatch<TDispatcher>()` only
  registered their interceptors in DI and never attached them to the `DbContext`. `UseEntityFramework` now adds the
  registered interceptors to the context options; an interceptor of the same type already added in `configure` is
  not added a second time (one added in `OnConfiguring` is not detected).
- `AuditInterceptor` turned repository deletes made with `permanent: true` (`DeleteAsync`, `DeleteRangeAsync`) on
  `ISoftDeletable` entities into soft deletes, so the row stayed in the database. Such deletes are now permanent. This
  also affected apps that added the interceptor by hand with `AddInterceptors(new AuditInterceptor())`.

### Changed — behaviour (apps calling `AddAuditInterceptor()` / `AddDomainEventDispatch<TDispatcher>()`)
- `DbContext.Remove` on an `ISoftDeletable` entity is now a soft delete (`IsDeleted = true`), as documented; before
  this release the row was deleted. Soft-deleted rows are hidden only from repository queries and from contexts that
  apply the soft-delete query filter; raw `DbSet` queries without it still return them, and unique indexes that do
  not exclude deleted rows can reject a re-insert.
- `CreatedDate` and `ModifiedDate` are now set by the interceptor on save; values you assign yourself are overwritten.
- Domain events are dispatched after `SaveChanges`, before an explicit transaction commits, and are cleared before
  dispatch: if the dispatcher throws, the data is already saved and the events are not retried. If you dispatched
  events manually as a workaround, remove it — they would now be dispatched twice. Prefer `SaveChangesAsync`; the
  synchronous path blocks a thread-pool thread while dispatching.

## [4.0.0] — 2026-10-27

Upgrade guide: [docs/migrations/v3-to-v4.md](docs/migrations/v3-to-v4.md) · Decision: [ADR-0028](docs/adr/0028-v4-scope.md).
3.6.x stays listed on nuget.org and receives security fixes from `release/3.x`.

### Removed — BREAKING
- `Kck.Pipeline.MediatR` (KCK0200) — MediatR 13+ requires a commercial licence. Use `Kck.Pipeline.Mediator` (MIT):
  [docs/migrations/mediatR-to-mediator.md](docs/migrations/mediatR-to-mediator.md). The licence gate no longer has a
  MediatR exception (ADR-0028 K1).
- `ITokenService.GetClaimsFromToken` / `JwtTokenService.GetClaimsFromToken` (KCK0001) — read claims without verifying
  the signature. Use `ValidateTokenAsync` (ADR-0028 K2).
- `Kck.EventBus.InMemory`: `AddKckEventBusInMemory()` and `AddKckEventBusInMemory(params Assembly[])`. Use
  `AddKckEventBus(b => b.UseInMemory().RegisterHandlersFromAssembly(...))` (ADR-0028 K4).

### Changed — BREAKING
- `IReadRepository<T, TId>` (KCK0100, ADR-0028 K3): the bool-flag read methods are gone; the `QueryOptions` methods
  that were extension methods in 3.x are now interface members with the same parameter order, and
  `ReadRepositoryExtensions` is removed. `options` is optional (`QueryOptions?`); when omitted, `GetAsync`,
  `GetListAsync` and `GetListByDynamicAsync` track entities and `AnyAsync`, `GetByIdAsync`, `CountAsync` do not — the
  same defaults as the 3.x bool overloads.
  - `repo.GetAsync(predicate, options)`, `repo.GetByIdAsync(id)`, `repo.CountAsync()` compile unchanged.
  - `repo.AnyAsync(predicate)`, `repo.CountAsync(predicate)`, `repo.GetListAsync(predicate)` (options come first):
    use `predicate:` or pass `QueryOptions.None` first.
  - Custom `IReadRepository` implementations must implement the new signatures.
  - **Tracking when migrating bool flags:** the presets do not track. `GetAsync`/`GetListAsync`/`GetListByDynamicAsync`
    with `withDeleted: true` (which tracked in 3.x) map to `new QueryOptions(IncludeDeleted: true, AsTracking: true)`,
    not to `QueryOptions.WithDeleted` — otherwise a load-modify-`SaveChanges` flow silently stops saving.

    | 3.x call | 4.0 equivalent |
    |---|---|
    | `GetAsync(p)` / `GetListAsync(...)` (tracking) | `GetAsync(p)` / `GetListAsync(...)` — unchanged |
    | `GetAsync(p, withDeleted: true)` | `GetAsync(p, new QueryOptions(IncludeDeleted: true, AsTracking: true))` |
    | `GetAsync(p, enableTracking: false)` | `GetAsync(p, QueryOptions.None)` |
    | `GetByIdAsync(id, withDeleted: true)` | `GetByIdAsync(id, QueryOptions.WithDeleted)` |
    | `AnyAsync(p)` / `CountAsync(p)` | `AnyAsync(predicate: p)` / `CountAsync(predicate: p)` |
- `Kck.Bundle.WorkerService` (ADR-0028 K5): Quartz is the bundled scheduler and the default; Hangfire (LGPL-3.0) is
  no longer referenced and `HangfireWorkerCount` is removed. `"Kck:BackgroundJobs:Provider": "Hangfire"` (the 3.x
  default) and unknown provider names now fail at startup with instructions instead of silently picking a scheduler.
  To keep Hangfire, reference `Kck.BackgroundJobs.Hangfire` and call `AddKckBackgroundJobsHangfire(...)` yourself.
- `Kck.Documents.ImageSharp` (ADR-0028 K6): an unsupported format name (e.g. `avif`) throws `NotSupportedException`
  instead of silently producing JPEG; `jpg` is accepted as `jpeg`. An empty/`null` format now keeps the source format,
  as `ImageProcessingOptions.OutputFormat` documents (3.x produced JPEG); sources Kck cannot encode (e.g. TIFF) still
  become JPEG.
- `Kck.Caching.Hybrid`: `HybridCacheService` no longer takes `IDistributedCache` (unused since the 3.6.2 `GetAsync`
  fix). Registration through `AddKckCachingHybrid` is unchanged.
- `Kck.BackgroundJobs.Quartz`: Quartz.NET 3.22.5 → 4.6.2. `AddKckBackgroundJobsQuartz(Action<IQuartzBuilder>?)`
  replaces `Action<IServiceCollectionQuartzConfigurator>?`; `Quartz.Extensions.DependencyInjection` / `.Hosting` are
  no longer needed. Quartz 4 behaviour changes (time zone, misfire, cancelled runs): `decisions/2026-10-27-quartz-4.md`.
- `Kck.Caching.Redis`, `Kck.Caching.Hybrid`, `Kck.Security.TokenBlacklist.Redis`: StackExchange.Redis 2.13 → 3.4.20.
  RESP3 is the default protocol; admin commands need `AllowAdmin`.
- Intentional binary breaks are listed per package in `CompatibilitySuppressions.xml` (package validation).

### Changed
- Dependencies: OpenTelemetry 1.15 → 1.20.1 (OTLP exporter no longer follows redirects; 64 MiB max request),
  YamlDotNet 16.3 → 18.1 (default recursion limit 130), FluentFTP 54.0.2 → 55.1.0 (security, also in 3.6.1).
- Documentation rewritten against the real API: persistence, background jobs, bundles, file storage, pipeline, package
  READMEs; the samples in the migration guide compile against 4.0.

### Fixed
- Includes the 3.6.1 (FluentFTP security) and 3.6.2 (`Kck.Caching.Hybrid` `GetAsync`) fixes.

## [3.6.2] — 2026-10-27

### Fixed
- `Kck.Caching.Hybrid`: `GetAsync` threw `JsonException` for every value written with `SetAsync`. HybridCache stores
  L2 entries in its own payload format, but `GetAsync` read them straight from `IDistributedCache` as JSON. `GetAsync`
  now reads through HybridCache (L1, then L2) and does not populate the cache on a miss. `GetOrSetAsync`,
  `ExistsAsync`, `RemoveAsync` and `RemoveByPrefixAsync` were not affected.

## [3.6.1] — 2026-10-27

### Security
- `Kck.FileStorage.FluentFtp`: FluentFTP 54.0.2 → 55.1.0. FluentFTP below 55.1.0 is affected by a high-severity advisory
  (FTP data exfiltration and spoofing through man-in-the-middle attacks, published 2026-10-18). No Kck API change;
  update to 3.6.1.

## [3.6.0] — 2026-10-27

### Added
- `Kck.FileStorage.AwsS3`: S3-compatible services (MinIO, Cloudflare R2, Wasabi). New `AwsS3Options.ServiceUrl`
  (custom endpoint; `Region` then only signs requests, validated as an absolute http(s) URL at startup),
  `ForcePathStyle` (path-style URLs) and `ChecksumWhenRequired` (compute/validate checksums only when the operation
  requires them, for services that reject the AWS SDK v4 default checksums). Defaults keep the Amazon S3 behaviour.
- Package validation: every package is checked against the last release (3.5.0) on `dotnet pack` and on every PR, so
  an accidental binary breaking change fails the build.

### Fixed
- `Kck.Templates` (`dotnet new kck-webapi`): the generated project now compiles. It referenced the unlisted
  `Kck.Bundle.WebApi` 3.0.0 (now always the same version as the template package), called Swagger APIs without a
  Swagger package (now `Microsoft.AspNetCore.OpenApi`: `AddOpenApi()`, `MapOpenApi()` in Development, document at
  `/openapi/v1.json`), and failed with `--IncludeSample false`.
- `Kck.FileStorage.AwsS3` README: the upload sample now uses the real `FileUploadRequest` API; the non-existent
  `PublicReadAccess` option is removed.

### Changed
- Dependencies: `Microsoft.EntityFrameworkCore` 10.0.14 (`Kck.Persistence.EntityFramework`),
  `Elastic.Clients.Elasticsearch` 9.5.3 (`Kck.Search.Elasticsearch`).
- Releases keep the newest version of each major listed on nuget.org; publishing 4.0.0 will not unlist the last 3.x.
- Licence gate: CI fails on any dependency whose licence is not on the permissive allow-list, unless it is a documented
  exception. No dependency requires a paid licence; see [docs/licensing.md](docs/licensing.md).

### Deprecated — removal in 4.0.0
4.0.0 (planned November–December 2026) removes the following; each already produces a compiler warning.
See [docs/policies/deprecation.md](docs/policies/deprecation.md).
- `Kck.Pipeline.MediatR` package (**KCK0200**) — MediatR 13+ requires a commercial licence. Move to
  `Kck.Pipeline.Mediator` (MIT): [docs/migrations/mediatR-to-mediator.md](docs/migrations/mediatR-to-mediator.md).
- `ITokenService.GetClaimsFromToken(string)` / `JwtTokenService.GetClaimsFromToken(string)` (**KCK0001**) — does not
  validate the signature.
- Old `IReadRepository<T>` overloads (**KCK0100**): `GetAsync`, `GetListAsync`, `GetListByDynamicAsync`, `AnyAsync`,
  `GetByIdAsync`, `CountAsync` — use the `QueryOptions` overloads.
- `Kck.EventBus.InMemory` legacy registration methods (marked `[Obsolete]`).

## [3.5.0] — 2026-10-15

### Fixed
- `Kck.Persistence.EntityFramework` — `BulkUpdateAsync` sent `object`-typed setters to `ExecuteUpdateAsync`; providers
  without an `object` type mapping (SQLite) failed. Setters are now typed per property. A value whose CLR type does
  not match its property exactly throws `ArgumentException`.

### Changed
- `Kck.Core.Abstractions` — `Entity<TId>.Id` is no longer `virtual`.
- `Kck.EventBus.Abstractions` — the `event` parameter of `IEventBus.PublishAsync` and `IEventHandler<TEvent>.HandleAsync`
  is renamed to `eventData`; calls that pass it by name (`@event:`) must be updated.
- `Kck.Documents.ImageSharp` — SixLabors.ImageSharp 3.1.12 → 4.1.3 (security; the vulnerability suppression is gone).
  See `decisions/2026-10-14-sdk-10.0.402-imagesharp-4.md` for the licence terms.
- Dependency security sweep: .NET SDK 10.0.402, Microsoft.Extensions 10.0.13 / 10.11.0, EF Core 10.0.13 and other
  patch updates.
- Packages are published with NuGet Trusted Publishing behind an approval gate; on each stable release older versions
  of the same major are unlisted.

## [3.4.0] — 2026-05-18

### Changed
- Every package has its own `README.nuget.md`, accurate release notes and signing information on nuget.org.

## [3.3.0] — 2026-05-18

### Changed
- Per-package NuGet metadata (description, tags, project and repository links) for all packages.

## [3.2.0] — 2026-05-17

### Removed
- `Kck.Core.Abstractions.Pipeline` — the `[Obsolete]` forwarding interfaces `ICachableRequest`, `ILoggableRequest`,
  `ISecuredRequest` and `ITransactionalRequest` (ADR-0026). Use the types in `Kck.Pipeline.Abstractions`. This
  removal in a minor version went against the deprecation policy; later removals happen only in a major version.

### Changed
- XML documentation for the public members of the provider packages.

## [3.1.0] — 2026-05-17

### Added
- `Kck.FeatureFlags.AzureAppConfig` — Azure App Configuration backed feature flags. Supports ConnectionString and Managed Identity (Endpoint). Configurable refresh interval, stampede-safe double-check locking, OWASP-style validator.
- `Kck.FileStorage.AzureBlob` — Azure Blob Storage `IFileStorageService` implementation. ConnectionString or Managed Identity. `CreateContainerIfNotExists` on startup. Full `Upload / Download / Delete / Exists / List` support.
- `Kck.FileStorage.AwsS3` — Amazon S3 `IFileStorageService` implementation. Explicit credentials or IAM role. Paginated `ListObjectsV2`. Optional `KeyPrefix` for multi-tenant isolation.
- `Kck.Templates` NuGet package — `dotnet new kck-webapi` project template pre-wired with `Kck.Bundle.WebApi` (Mediator, caching, JWT, Argon2, Serilog, OpenTelemetry, rate limiting, exception handling). `--IncludeSample false` to skip WeatherForecast sample.
- `Azure.Storage.Blobs 12.28.0`, `AWSSDK.S3 4.0.13`, `Azure.Data.AppConfiguration 1.9.0` → `Directory.Packages.props`.

## [3.0.0] — 2026-05-17

### Added — ADR-0022 / ADR-0023 / ADR-0024 / ADR-0025 / ADR-0026
- `Kck.Caching.Hybrid` — `Microsoft.Extensions.Caching.Hybrid` (HybridCache) provider; L1+L2 two-tier caching (ADR-0020).
- `docs/adr/0022-di-lifetime-strategy.md` — Singleton/Scoped/Transient rules for all framework services.
- `docs/adr/0023-argon2-built-in-assessment.md` — .NET 10 has no built-in Argon2; Konscious.Argon2 retained.
- `docs/adr/0024-entity-hierarchy.md` — Entity\<TId\> hierarchy rationale.
- `docs/adr/0025-paginate-record-result-unchanged.md` — Paginate sealed record, Result\<T\> unchanged.
- `docs/adr/0026-pipeline-abstractions-package.md` — Kck.Pipeline.Abstractions package rationale.
- `Kck.Pipeline.Abstractions` — new package with `ICachableRequest`, `ILoggableRequest`, `ISecuredRequest`, `ITransactionalRequest`. Pipeline behaviors migrated to reference this package directly.
- `IValidateOptions<T>` + `ValidateOnStart()` for 10 providers: MailKit, SendGrid, AmazonSes, AzureServiceBus, Elasticsearch, JWT, AzureKeyVault, FluentFtp, RedisTokenBlacklist, Argon2 — with rich pit-of-success error messages.
- XML `<summary>` documentation for all 11 Abstractions packages; CS1591 enforced as error per-package.
- `docs/faq.md` — 13 categories of common questions and solutions.
- `.github/workflows/sonar.yml` — SonarCloud PR decoration (requires `SONAR_TOKEN` secret).
- `.github/workflows/benchmark.yml` — BenchmarkDotNet regression CI; 10%+ slowdown → fail.
- `.github/workflows/mutation.yml` — weekly mutation testing (Core, Caching, Security modules).
- `docfx.json` + `toc.yml` + `.github/workflows/docs.yml` — DocFX GitHub Pages API site.
- `stryker-caching.json`, `stryker-security.json` — Stryker.NET configs for additional modules.
- `[DebuggerDisplay]` on `Entity<TId>`, `IntegrationEvent`, `InMemoryCacheService`, `RedisCacheService`, `HybridCacheService`.
- `docs/migrations/entity-hierarchy-v3.md` — step-by-step guide for the Entity\<TId\> → FullEntity\<TId\> migration.
- `PiiMaskingEnricher` — Serilog enricher that redacts named log properties (ADR-0021 area).
- 80+ new unit tests (EntityTests, PaginateTests, ErrorTests, property-based with CsCheck, validator tests for 10 providers).
- AOT attributes: `[DynamicallyAccessedMembers(PublicProperties)]` on `ICsvExporter`/`IExcelService` generics; `[RequiresUnreferencedCode]` on `GlobalExceptionHandler.ResolveStatusCode`.

### Changed — BREAKING
- **`Entity<TId>`** no longer implements `IAuditable` or `ISoftDeletable`. Migrate to `FullEntity<TId>` (drop-in replacement) or `AuditableEntity<TId>`. See `docs/migrations/entity-hierarchy-v3.md` (ADR-0024).
- **`Paginate<T>`** is now `sealed record Paginate<T>`. Structural equality replaces reference equality. `with` operator is now available (ADR-0025).
- **`Kck.Core.Abstractions.Pipeline.*`** interfaces are `[Obsolete]` — use `Kck.Pipeline.Abstractions.*` instead. Will be removed in v3.1 (ADR-0026).

### Deprecated
- `Kck.Core.Abstractions.Pipeline.ICachableRequest` / `ILoggableRequest` / `ISecuredRequest` / `ITransactionalRequest` — use `Kck.Pipeline.Abstractions.*`. Will be removed in v3.1.
- `Kck.Pipeline.MediatR` — **KCK0200** warning. Use `Kck.Pipeline.Mediator`. Migration: `docs/migrations/mediatR-to-mediator.md` (ADR-0021).

### Security
- SBOM (CycloneDX JSON) generated on every NuGet release as artifact.
- Coverage threshold gate: Line ≥ 43%, Branch ≥ 38% (enforced in CI).

## [2.0.0] — 2026-05-07

### Removed — BREAKING
- **Target framework support removed** (ADR-0019). The library now targets only `net10.0` and later. This is a
  **breaking change**; end of support date 2026-11-10.
- `#if NET9_0_OR_GREATER` fallback blocks (`IntegrationEvent.Id`, `QuartzJobScheduler.NewJobId()`, `TotpMfaProvider`
  secret hash) removed.

### Changed
- `Mediator.Abstractions` updated to the stable 3.0.2 (previously 3.1.0-prerelease).

## [1.1.0] — 2026-05-03

### Added
- `Kck.Pipeline.Mediator` — pipeline builder on `Mediator.Abstractions`; 5 built-in behaviors: Authorization, Caching,
  Logging, Transaction, Validation. Registered with the `AddKckMediator()` DI extension (Library Strategy §1.3, FAZ-9).
- `Kck.Hosting.Aspire` — .NET Aspire host integration; `AddKckServiceDefaults()` configures health check, resilience and
  observability defaults in one line (Library Strategy §10.2, FAZ-9).

## [1.0.0] — 2026-05-03

### Added (LS-FAZ-8)

- `Kck.Resilience.Polly` — wrapper over `Microsoft.Extensions.Resilience` (Polly v8). Named pipelines registered with
  `AddKckResilience("pipeline-name", builder => ...)`, resolved through `ResiliencePipelineProvider<string>` (Library
  Strategy §3.1, ADR-0018).
- `src/Abstractions/Directory.Build.props`: `<IsAotCompatible>true</IsAotCompatible>` applied to all 16 abstraction
  projects — 0 new trim/AOT warnings (Library Strategy §2.2, ADR-0018).
- `docs/adr/0018-v1-breaking-changes.md` — rationale for the v1.0 breaking changes and migration guide.

### Changed (LS-FAZ-8) — BREAKING

- **`Paginate<T>` immutability:** all 11 properties `{ get; set; }` → `{ get; init; }`. Setting them after construction
  is now a compile error. Migration: use the `Paginate<T>.Create(...)` factory (Library Strategy §4.1, ADR-0018).
- **`ICacheService.GetAsync<T>` / `ExistsAsync`** return `ValueTask<T?>` / `ValueTask<bool>` instead of `Task<T?>` /
  `Task<bool>`. Call sites do not change; only `ICacheService` implementations update their override signatures
  (Library Strategy §5.3, ADR-0018).

### Added (LS-FAZ-7)

- `Kck.Bundle.WorkerService` — opinionated stack for Worker Service hosts: Serilog + OpenTelemetry + in-memory EventBus +
  Hangfire **or** Quartz (both together are rejected; chosen with `Kck.BackgroundJobs.Provider`) (Library Strategy §1.1,
  ADR-0017).
- `Kck.Bundle.MinimalApi` — slim variant of WebApi: Serilog + in-memory cache + JWT + exception handling + health checks;
  MVC, the MediatR pipeline and Argon2 deliberately left out (Library Strategy §1.1, ADR-0017).
- `RedisHealthCheck` — PING through `IConnectionMultiplexer`; registered automatically by `AddKckCachingRedis` (tagged
  `ready`) (Library Strategy §10.3, ADR-0017).
- `EfCoreHealthCheck<TContext>` — based on `CanConnectAsync()`; registered manually with
  `AddHealthChecks().AddKckEfCoreCheck<MyContext>()` (Library Strategy §10.3, ADR-0017).
- `RabbitMqHealthCheck` — opens a short-lived connection; registered automatically by `UseRabbitMq` (tagged `ready`)
  (Library Strategy §10.3, ADR-0017).
- `schemas/appsettings.kck.schema.json` — JSON Schema draft-07 with every provider configuration key, types and
  examples; IDE autocomplete through `"$schema"` (Library Strategy §10.4, ADR-0017).
- `docs/configuration-schema.md` — a field table per provider + a complete `appsettings.json` example (Library Strategy
  §10.4).
- `.github/release-drafter.yml` + `.github/workflows/release-drafter.yml` — release note drafts created automatically when
  a PR merges; `feat` → minor, `breaking` → major, others → patch (Library Strategy §6.3, ADR-0017).
- `docs/adr/0017-bundle-health-release.md` — bundle, health check and release decisions.

### Added (LS-FAZ-6)

- `ReadRepositoryExtensions` static class — 6 `QueryOptions`-based extension methods on `IReadRepository<T,TId>`:
  `GetAsync`, `GetListAsync`, `GetListByDynamicAsync`, `AnyAsync`, `GetByIdAsync`, `CountAsync`. A self-documenting API
  instead of positional `bool withDeleted / bool enableTracking` (Library Strategy §4.2, ADR-0016).
- `QueryOptions.None` static property — an explicitly named factory for `QueryOptions(false, false)`, next to
  `Tracking` and `WithDeleted` (Library Strategy §4.2).
- `ResultExtensions` static class — functional pipeline for `Result<T>`: `Map<U>`, `Bind<U>`, `Tap`, `Ensure` (Library
  Strategy §4.3, ADR-0016).
- `docs/adr/0016-queryoptions-api.md` — QueryOptions API + Result functional pipeline decision.

### Changed (LS-FAZ-6)

- **Deprecation:** the 6 bool-parameter methods of `IReadRepository<T,TId>` are marked
  `[Obsolete(DiagnosticId = "KCK0100")]`. The `EfRepository` implementation is unchanged — KCK0100 warns only callers
  that go through the interface (Library Strategy §4.2, ADR-0016).
- **Performance:** per-key locking in `CacheServiceBase` uses a 64-stripe lock array instead of an unbounded
  `ConcurrentDictionary<string, SemaphoreSlim>` — removes the memory leak and GC pressure in long-lived cache services
  (Library Strategy §5.2).

### Added (LS-FAZ-5)
- `Filter` API: a `Filter(string field, FilterOperator op, ...)` constructor (turns the enum into the canonical
  lower-case wire string), an `OperatorEnum` getter and a `TryParseOperator` static helper. The wire format is unchanged —
  JSON consumers are not affected (Library Strategy §4.4).
- `Filter.GetValue<T>` rewritten to be AOT-friendly: a switch + type-specific `ParseString<T>` instead of
  `Convert.ChangeType`. `JsonElement` is now handled with native `Deserialize<T>`; `Guid`, `DateTime`, `DateTimeOffset`,
  `DateOnly`, `TimeOnly` and `Enum` are supported; unknown types fall back to `Convert.ChangeType` (Library Strategy
  §5.5).
- `docs/adr/0015-perf-quickwins.md` — Redis EXISTS + Filter type safety + AOT decision.

### Changed (LS-FAZ-5)
- **Performance:** `RedisCacheService.ExistsAsync` now uses `IConnectionMultiplexer.GetDatabase().KeyExistsAsync()` (the
  `EXISTS` command only) — the old `IDistributedCache.GetStringAsync()` fetched the whole value over the network. 95%+
  less network/GC on caches with large values (Library Strategy §5.6, P1).
- `DynamicFilterExtensions.ApplyFilter` now uses `filter.OperatorEnum ?? throw` — it used to run
  `Enum.Parse<FilterOperator>(...)` on every call.

### Added (LS-FAZ-4)
- `tests/Kck.Benchmarks` — BenchmarkDotNet 0.15.0 console app + 3 initial benchmarks (`PaginateCreateBenchmarks`,
  `ResultBenchmarks`, `JsonSerializationBenchmarks`). Baseline for the LS-FAZ-5/6 performance work (Library Strategy
  §5.7).
- `tests/Kck.Persistence.EntityFramework.Tests/EfRepositoryIntegrationTests.cs` — Testcontainers proof of concept
  (postgres:16-alpine), `[Trait("Category", "Integration")]`. Catches real-database behavior that mocks and the InMemory
  provider miss (Library Strategy §7.5).
- `Testcontainers.PostgreSql 4.5.0` + `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0` → `Directory.Packages.props` (used only
  by the EF test project).
- `docs/test-strategy.md` — test pyramid guide (unit vs integration vs benchmark, when to use which, current coverage
  matrix).
- `docs/policies/test-coverage.md` — coverage gate policy + gradual raise roadmap (40/35 → 50/45 → 65/50 → 75/60).
- `docs/adr/0014-test-strategy.md` — BenchmarkDotNet + Testcontainers + coverage policy decision.
- README "Testing" + "Benchmarks" sections (test commands with and without Docker).

### Changed (LS-FAZ-4)
- CI `build-test.yml` test step split in two: `Test (unit)` (filter `Category!=Integration`, Ubuntu and Windows) and
  `Test (integration)` (filter `Category=Integration`, Ubuntu only — Docker needed).
- CI coverage gate: `BRANCH_THRESHOLD: 30 → 35` (prevents regression; measured 36.3%, 1.3 points of headroom). Line gate
  stays at 40% (measured 40.7%).

### Added (LS-FAZ-3)
- `Microsoft.CodeAnalysis.PublicApiAnalyzers 4.14.0` — enabled in 16 abstraction projects (ADR-0013). New public API
  needs a line in `PublicAPI.Unshipped.txt`, otherwise the build fails with RS0016.
- `PublicAPI.Shipped.txt` baseline in every abstraction — 875 unique public symbols recorded.
- `src/Abstractions/Directory.Build.props` — applies the analyzer to all 16 abstractions in one place (re-imports the root
  Directory.Build.props).
- `docs/policies/support.md` — .NET support matrix (LTS/STS), process for adding/removing target frameworks (Library
  Strategy §6.4).
- `docs/policies/versioning.md` — SemVer contract, definition of "public API", breaking change policy, tracking with
  PublicApiAnalyzers (Library Strategy §12.3).
- `docs/migrations/README.md` + `_template.md` — per-major migration guide template (Library Strategy §9.4).
- `docs/adr/0013-public-api-discipline.md` — PublicApiAnalyzers + SemVer + Obsolete discipline decision.
- `[DebuggerDisplay]` — `Result`, `Result<T>`, `Error`, `Paginate<T>`, `PageRequest` (Library Strategy §9.3); a meaningful
  summary instead of the class name in the watch window.
- ADR-0011/0012/0013 added to the `docs/adr/README.md` index (left over from LS-FAZ-2).

### Changed (LS-FAZ-3)
- `Kck.Caching.Redis` missing-configuration error message rewritten in "pit of success" style: concrete examples of the 3
  alternatives + a documentation link (Library Strategy §9.2 literal example).

### Added (LS-FAZ-2)
- Multi-target build: `Kck.*.Abstractions` + plain providers now target `net8.0` LTS + `net10.0` STS (ADR-0011).
  Net10-only opt-out packages: `Kck.Bundle.WebApi`, `Kck.Persistence.EntityFramework`, `Kck.AspNetCore`,
  `Kck.Security.Jwt`, `Kck.Caching.Redis`, `Kck.Http.Resilience`, `Kck.Exceptions.AspNetCore`.
- `docs/adr/0011-multi-target-net8-net10.md` — multi-target decision and net8 fallback table.
- `docs/adr/0012-remove-automapper-package.md` — decision to remove the unused AutoMapper package.
- README "Quick Start" in 3 tabs: WebApi (bundle), MinimalApi (slim), WorkerService (no HTTP).
- README "Why Kck?" comparison section (vs ABP / FastEndpoints / Aspire).
- README background jobs tip: Hangfire or Quartz; using both is not recommended.

### Changed (LS-FAZ-2)
- `Microsoft.IdentityModel.JsonWebTokens 8.17.0` pin explained in a comment — diamond-dependency alignment (Library
  Strategy §2.3 item 2).
- Net8 fallbacks added (`#if NET9_0_OR_GREATER`): `IntegrationEvent.Id`, `QuartzJobScheduler.NewJobId()`,
  `TotpMfaProvider` secret hash. Net8 uses `Guid.NewGuid()` and `Convert.ToHexString().ToLowerInvariant()`; net9+ uses
  `Guid.CreateVersion7()` and `Convert.ToHexStringLower()`.

### Removed (LS-FAZ-2)
- `AutoMapper 16.1.1` PackageVersion entry removed from `Directory.Packages.props` (no usages, ADR-0012).

### Added
- `SECURITY.md` — security policy with CVSS-based response SLA and 90-day coordinated disclosure (Library Strategy §13.1).
- `docs/policies/deprecation.md` — formal deprecation policy with `KCK0001-0999` DiagnosticId numbering (ADR-0010).
- `docs/adr/0010-deprecation-policy.md` — records the obsoletion lifecycle decision.
- Source Link & deterministic build enabled via `Microsoft.SourceLink.GitHub 8.0.0`; `.snupkg` symbol packages now produced (Library Strategy §6.1).
- `.github/workflows/license-audit.yml` — weekly NuGet license scan; blocks GPL/AGPL/LGPL/SSPL/BUSL contamination (Library Strategy §13.6).
- CI: `dotnet list package --vulnerable --include-transitive` step; Critical/High CVE → fail (Library Strategy §13.2).
- `FtpConnectionPool` for `Kck.FileStorage.FluentFtp` — pooled `AsyncFtpClient` connections with configurable `PoolSize`.
- `QueryOptions` readonly record struct in `Kck.Persistence.Abstractions` — opt-in replacement for positional `bool withDeleted / enableTracking` flags.
- `Paginate<T>.Create` static factory that replaces the removed internal synchronous constructor.
- `AddKckJob<TJob>()` helper for Hangfire and Quartz providers.
- `Kck.Exceptions.Abstractions` package split out from `Kck.Exceptions`.
- ADRs 0001–0009 under `docs/adr/`.

### Changed (LS-FAZ-1)
- `JwtTokenService.GetClaimsFromToken` `[Obsolete]` attribute now ships with `UrlFormat` pointing to `docs/policies/deprecation.md` (KCK0001).
- `nuget-publish.yml` no longer passes `/p:ContinuousIntegrationBuild=true` — moved to `Directory.Build.props` under `GITHUB_ACTIONS` condition.

### Changed
- **BREAKING:** `IOptions<T>` → `IOptionsMonitor<T>` across all provider services (ADR-0004).
- **BREAKING:** `EfUnitOfWork` no longer uses service-locator pattern; replaced with `IEfRepositoryFactory` (ADR-0005).
- **BREAKING:** `Kck.Caching.Redis` registration now uses async `ConnectAsync` + `IHostedService` (ADR-0006).
- **BREAKING:** `FluentFtpService` constructor now depends on `FtpConnectionPool` instead of `IOptionsMonitor<FluentFtpOptions>`.
- `EfRepository.DeleteRangeAsync` rewritten from `foreach + await DeleteAsync` (N async allocations) to a single-pass state-setting loop.
- `RedisCacheService.RemoveByPrefixAsync` now chunks deletes into 1000-key batches.
- `SmtpConnectionPool._disposed` uses `Interlocked.CompareExchange` for thread-safe disposal.
- `TryAddSingleton` used consistently across provider DI registrations (19 files).
- `JwtTokenService` migrated to `JsonWebTokenHandler` (ADR-0003).
- `MailKit` upgraded to 4.16.0 (CVE GHSA-9j88-vvj5-vhgr).

### Removed
- `Hangfire.MySqlStorage` dependency (ADR-0002).
- `JwtTokenService.GetClaimsFromToken` marked `[Obsolete]` — does not verify signature.

### Security
- Samples no longer fall back to `"guest"` when RabbitMQ credentials are absent — fail fast instead.
- `OpenTelemetry.Instrumentation.EntityFrameworkCore` added for DB-call tracing.
- `CacheServiceBase` semaphore eviction race removed.
- `EventBus` providers (`InMemory`, `RabbitMq`, `AzureServiceBus`) now propagate `OperationCanceledException` correctly.
- `PathSanitizer` rejects URL-encoded (`%2e%2e`, `..%2f`) traversal attempts.

## [0.1.0] — Initial public cut

First public drop of the Kck modular architecture framework.
