# Architecture Decision Records (ADR)

This folder holds the records of the project's significant architecture decisions. ADRs are numbered and immutable: a
decision that changes later is replaced by a new ADR and the old one is marked `superseded`. ADR-0001 to ADR-0028 are
historical records in their original language (Turkish); ADRs from 0029 on are written in English (ADR-0029).

## Status

| ADR | Title | Status | Date | Scope |
|---|---|---|---|---|
| [0001](0001-argon2-implementation.md) | Argon2 implementation — package kept | Accepted | 2026-04-20 | `Kck.Security.Argon2` |
| [0002](0002-hangfire-storage.md) | Hangfire storage simplified (MySqlStorage removed) | Accepted | 2026-04-20 | `Kck.BackgroundJobs.Hangfire` |
| [0003](0003-jwt-handler-migration.md) | `JwtSecurityTokenHandler` → `JsonWebTokenHandler` | Accepted | 2026-04-20 | `Kck.Security.Jwt` |
| [0004](0004-ioptions-monitor-migration.md) | `IOptions<T>` → `IOptionsMonitor<T>` migration | Accepted | 2026-04-20 | All providers |
| [0005](0005-ef-repository-factory.md) | `EfUnitOfWork` service locator → `IEfRepositoryFactory` | Accepted | 2026-04-20 | `Kck.Persistence.EntityFramework` |
| [0006](0006-redis-async-hosted-service.md) | Redis `ConnectAsync` + `IHostedService` | Accepted | 2026-04-20 | `Kck.Caching.Redis`, `Kck.Security.TokenBlacklist.Redis` |
| [0007](0007-add-kck-job-helper.md) | `AddKckJob<TJob>()` helper | Accepted | 2026-04-20 | `Kck.BackgroundJobs.*` |
| [0008](0008-exceptions-abstractions-split.md) | `Kck.Exceptions` → `Abstractions` split | Accepted | 2026-04-20 | `Kck.Exceptions.*` |
| [0009](0009-tryaddsingleton-consistency.md) | `TryAddSingleton` consistency | Accepted | 2026-04-20 | 19 providers |
| [0010](0010-deprecation-policy.md) | Deprecation policy + DiagnosticId numbering | Accepted | 2026-04-25 | All public API |
| [0011](0011-multi-target-net8-net10.md) | Multi-target net8.0 + net10.0 | ~~Accepted~~ Superseded (ADR-0019) | 2026-04-26 | Abstractions + plain providers |
| [0012](0012-remove-automapper-package.md) | AutoMapper package removed (unused) | Accepted | 2026-04-26 | `Directory.Packages.props` |
| [0013](0013-public-api-discipline.md) | Public API discipline (PublicApiAnalyzers + SemVer) | Accepted | 2026-04-26 | 16 abstraction projects |
| [0014](0014-test-strategy.md) | Test strategy (BenchmarkDotNet + Testcontainers + coverage policy) | Accepted | 2026-04-26 | All test projects + benchmarks |
| [0015](0015-perf-quickwins.md) | Performance quick wins (Redis EXISTS + Filter type safety) | Accepted | 2026-04-26 | `Kck.Caching.Redis`, `Kck.Persistence.Abstractions` |
| [0016](0016-queryoptions-api.md) | QueryOptions API — remove IReadRepository bool flags | Accepted | 2026-05-03 | `Kck.Persistence.Abstractions`, `Kck.Core.Abstractions` |
| [0017](0017-bundle-health-release.md) | Bundle strategy, health check pattern and release automation | Accepted | 2026-05-03 | `Kck.Bundle.*`, 3 providers, CI |
| [0018](0018-v1-breaking-changes.md) | v1.0 breaking changes — Paginate immutability, ValueTask, Resilience, AOT | Accepted | 2026-05-03 | `Kck.Core.Abstractions`, `Kck.Caching.*`, `Kck.Resilience.Polly` |
| [0019](0019-drop-net8-target.md) | net8.0 support dropped | Accepted | 2026-05-04 | All projects |
| [0020](0020-hybridcache-provider.md) | Kck.Caching.Hybrid (HybridCache L1+L2) | Accepted | 2026-05-07 | `Kck.Caching.Hybrid` |
| [0021](0021-deprecate-mediatR-pipeline.md) | Kck.Pipeline.MediatR deprecation (KCK0200) | Accepted | 2026-05-07 | `Kck.Pipeline.MediatR` |
| [0028](0028-v4-scope.md) | v4.0.0 scope — announced removals, licence of the default path | Accepted | 2026-10-27 | `Kck.Pipeline.MediatR`, `Kck.Persistence.Abstractions`, `Kck.Security.*`, `Kck.EventBus.InMemory`, `Kck.Bundle.WorkerService`, `Kck.Documents.ImageSharp` |
| [0029](0029-dx-first-adoption.md) | Adoption: developer experience first, Persistence as the hero module | Accepted | 2026-10-30 | `Kck.Templates`, `docs/`, `Kck.Persistence.EntityFramework` |
| [0030](0030-domain-event-handlers.md) | Built-in domain event handlers (`IDomainEventHandler<T>`, `AddDomainEventHandlers`) | Accepted | 2026-10-30 | `Kck.Core.Abstractions`, `Kck.Persistence.EntityFramework` |
| [0031](0031-persistence-analyzers.md) | Roslyn analyzers shipped with the persistence package (KCK1001, KCK1002) | Accepted | 2026-10-30 | `Kck.Persistence.EntityFramework` |
| [0032](0032-package-dependency-surface.md) | Package dependency surface: no `Microsoft.AspNetCore.App` framework reference unless the code needs it | Accepted | 2026-11-01 | 26 providers and bundles, `Kck.BackgroundJobs.Hangfire`, `Kck.Logging.Serilog` |

## By category

### Security
- [0001](0001-argon2-implementation.md) — password hashing package
- [0003](0003-jwt-handler-migration.md) — modern JWT handler

### Dependency risk
- [0002](0002-hangfire-storage.md) — abandoned package removed
- [0012](0012-remove-automapper-package.md) — unused package removed

### Interface / DI design
- [0004](0004-ioptions-monitor-migration.md) — options reload semantics
- [0005](0005-ef-repository-factory.md) — service locator anti-pattern
- [0006](0006-redis-async-hosted-service.md) — async lifecycle
- [0007](0007-add-kck-job-helper.md) — DI registration ergonomics
- [0008](0008-exceptions-abstractions-split.md) — abstraction leakage
- [0009](0009-tryaddsingleton-consistency.md) — multiple-registration behavior

### Policy / governance
- [0010](0010-deprecation-policy.md) — public API obsoletion lifecycle
- [0013](0013-public-api-discipline.md) — public API tracking + SemVer

### Build / target framework
- [0011](0011-multi-target-net8-net10.md) — net8.0 LTS + net10.0 STS (superseded)
- [0019](0019-drop-net8-target.md) — net8.0 dropped, net10.0 the only target
- [0032](0032-package-dependency-surface.md) — package dependency surface (no shared-framework reference unless needed)

### Test / quality
- [0014](0014-test-strategy.md) — BenchmarkDotNet + Testcontainers + coverage policy

### Performance
- [0015](0015-perf-quickwins.md) — Redis EXISTS + Filter type safety + AOT-friendly GetValue<T>

### Public API & developer experience
- [0016](0016-queryoptions-api.md) — IReadRepository QueryOptions API + Result<T> functional pipeline
- [0029](0029-dx-first-adoption.md) — developer experience first, Persistence as the hero module
- [0030](0030-domain-event-handlers.md) — built-in domain event handlers
- [0031](0031-persistence-analyzers.md) — persistence analyzers (KCK1001, KCK1002)

### Release / ecosystem
- [0017](0017-bundle-health-release.md) — WorkerService & MinimalApi bundles, release-drafter, health check pattern
- [0028](0028-v4-scope.md) — v4.0.0 scope

## Writing a new ADR

Use the next free number and this structure (see ADR-0029 for a recent example):

```markdown
# ADR-NNNN — <decision title>

**Status:** Proposed | Accepted | Rejected | Superseded
**Date:** YYYY-MM-DD

## Context
Why is a decision needed?

## Decision
What will be done?

## Alternatives considered
Why this option, and why the others were rejected.

## Consequences
Positive and negative effects of the decision.
```

**Required for:** a new framework, a major version upgrade, an architecture pattern change, a database change, an
authentication change, a new external service.
