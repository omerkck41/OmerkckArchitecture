# Persistence comparison and "Why Kck?" table — sources (plan G2.5)

Date: 2026-10-30 · Status: research · Constitution #1, #3, #4

## Pre-commitment (written before the research)

**What is compared:** the persistence concerns Kck's hero module covers — soft delete (query filter and delete
conversion, cascade to dependants), audit fields, domain events dispatched after commit, repository / unit of work,
specifications — in: EF Core 10 alone (built-in features), a hand-written solution, ABP Framework, Ardalis.Specification.
Plus every row of the README "Why Kck?" table (ABP, FastEndpoints, .NET Aspire).

**Questions:**
1. EF Core 10: named query filters and `IgnoreQueryFilters(names)`; `SaveChangesInterceptor`; what EF does *not*
   provide (soft-delete conversion of deletes, audit stamping, domain event dispatch).
2. ABP Framework: current major version, licence, target frameworks, Native AOT support; soft delete (`ISoftDelete`,
   data filters), audit properties, domain / distributed events and when they are published, repositories / UoW.
3. Ardalis.Specification: current version, licence, target frameworks, what it provides.
4. FastEndpoints: licence, target frameworks, Native AOT.
5. .NET Aspire: licence, target frameworks, scope.

**Criteria:**
- Only primary sources: official documentation, the project's repository, its NuGet package page. Each claim records
  URL, access date and the version it applies to.
- A claim that cannot be verified from a primary source is left out of the pages, not softened.
- Each competitor's strengths are written down too (plan R15); subjective rows ("learning curve") are dropped.
- Kck's own claims are checked against this repository, not against its README.

## Findings (all accessed 2026-10-30)

### Versions, licences, target frameworks (nuget.org package pages)
| Package | Latest stable | Licence | Target frameworks | Source |
|---|---|---|---|---|
| Volo.Abp.Core (ABP) | 10.7.0 (11.0.0-rc.1 in preview) | LGPL-3.0-only | netstandard2.0, netstandard2.1, net8.0, net9.0, net10.0 | https://www.nuget.org/packages/Volo.Abp.Core |
| Ardalis.Specification | 9.3.1 | MIT | netstandard2.0, net8.0, net9.0 | https://www.nuget.org/packages/Ardalis.Specification |
| FastEndpoints | 8.3.0 (9.0.0-beta in preview) | MIT | net8.0, net9.0, net10.0 | https://www.nuget.org/packages/FastEndpoints |
| Aspire.Hosting.AppHost (Aspire) | 13.6.4 | MIT | net8.0, net9.0, net10.0 | https://www.nuget.org/packages/Aspire.Hosting.AppHost |

- Ardalis.Specification README: "Ardalis.Specification is moving to a commercial license." — no date or version given;
  the current package is MIT. https://github.com/ardalis/Specification (README).
- Aspire is now named "Aspire" (not ".NET Aspire"): "a code-first orchestration and observability layer for
  distributed applications". https://aspire.dev/get-started/what-is-aspire/ (updated 2026-06-01).

### Native AOT
- FastEndpoints: supported with `FastEndpoints.Generator` (source-generated JSON contexts); some cases fall back to
  reflection. https://fast-endpoints.com/docs/native-aot
- ABP: no official statement that an ABP application can be published with Native AOT (search of abp.io, 2026-10-30).
  → Written as "not documented", not as "no".
- Aspire: an orchestration layer; AOT of the AppHost is not a meaningful comparison → row marked not applicable.

### EF Core 10 (https://learn.microsoft.com/ef/core/querying/filters, updated 2026-06-24)
- Named filters (EF 10): `HasQueryFilter("SoftDeletionFilter", ...)`, selective `IgnoreQueryFilters(["..."])`.
- "Filters can only be defined for the root entity type of an inheritance hierarchy."
- Soft delete: EF provides the filter; turning deletes into updates is left to the application (`SaveChangesAsync`
  override example in the docs). Audit stamping and domain event dispatch are not EF features.

### ABP Framework 10.7 (abp.io/docs/10.7)
- Data filtering: `ISoftDelete`; repository deletes set `IsDeleted`; `HardDeleteAsync`; filter on by default;
  `IDataFilter.Disable<ISoftDelete>()`; built-in `IMultiTenant` filter.
  https://abp.io/docs/latest/framework/infrastructure/data-filtering
- Auditing: `CreationTime`, `CreatorId`, `LastModificationTime`, `LastModifierId`, `DeletionTime`, `DeleterId`;
  "ABP automatically manages these properties wherever possible."
  https://abp.io/docs/latest/framework/architecture/domain-driven-design/entities
- Local events: handlers run "just before the current unit of work completed", inside the transaction; a handler
  exception rolls the unit of work back. https://abp.io/docs/latest/framework/infrastructure/event-bus/local
- Distributed events: transactional outbox and inbox (EF Core or MongoDB, distributed lock required).
  https://abp.io/docs/latest/framework/infrastructure/event-bus/distributed
- Interception: "implemented on top of the Castle DynamicProxy library" (UOW, auditing, authorization).
  https://abp.io/docs/latest/framework/infrastructure/interceptors
- Specifications: `Specification<T>` (`ToExpression()`), `And` / `Or` / `Not` / `AndNot`, translated to SQL by
  repositories. https://abp.io/docs/latest/framework/architecture/domain-driven-design/specifications

### Ardalis.Specification 9.3.1
- Query specifications, `SpecificationEvaluator`, a built-in EF Core `RepositoryBase`. Soft delete, auditing and
  domain events are not part of it (not mentioned in the README).

### Kck (this repository, main 422a104)
- 37 provider packages under `src/Providers` (the `Kck.Pipeline.MediatR` folder is a build leftover, not a package),
  17 abstraction packages, 3 bundles.
- Persistence: repository / `AuditInterceptor` soft delete following EF delete rules, `RevertSoftDeleteAsync`, named
  `KckSoftDelete` filter that keeps other named filters, audit timestamps (`CreatedBy` / `ModifiedBy` / `DeletedBy`
  exist but are not set automatically), domain events after commit (at-least-once; no outbox), no multi-tenancy.
  `Kck.Persistence.Abstractions` has `ISpecification<T>` / `Specification<T>`, but no repository method or evaluator
  applies them.
- Uses source generators where they help (`LoggerMessage`, the Mediator generator for the pipeline), no runtime proxies.

## Decision
- README "Why Kck?": versions and target frameworks updated; ABP Native AOT "not documented"; subjective rows
  (learning curve, batteries included) and unverifiable rows (provider counts of others, Source Link) removed;
  "no source generation" corrected to "no runtime proxies". The runtime-proxies cell is "—" for FastEndpoints and
  Aspire: neither documents interception either way, so no claim is made.
- New page `docs/persistence-comparison.md` with the persistence matrix, the competitors' strengths, and a link here.
