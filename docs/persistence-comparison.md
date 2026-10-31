# Persistence: Kck compared

What `Kck.Persistence.EntityFramework` adds around EF Core, and how the same needs are met with EF Core alone, a
hand-written layer, ABP Framework and Ardalis.Specification. Versions and sources were checked on 2026-10-30; every
claim about another project links to its documentation in
[the sources record](https://github.com/omerkck41/OmerkckArchitecture/blob/main/decisions/2026-10-30-persistence-comparison-sources.md).

| Need | EF Core 10 alone | Hand-written | ABP Framework 10.7 | Ardalis.Specification 9.3.1 | Kck 4.x |
|---|---|---|---|---|---|
| Hide soft-deleted rows | Named query filter (`HasQueryFilter(name, ...)`) | Same | `ISoftDelete` data filter, on by default | — | `ApplyGlobalSoftDeleteQueryFilter()` (named `KckSoftDelete`) and repository queries |
| Turn deletes into soft deletes | You write it (`SaveChanges` override) | You write it | Repository deletes set `IsDeleted`; `HardDeleteAsync` | — | Repository and `DbContext.Remove` (audit interceptor); `permanent: true` |
| Soft-delete dependants by the model's delete rules | — | You write it | Not documented | — | Follows `DeleteBehavior` (cascade, set null, restrict) |
| Include deleted rows in one query | `IgnoreQueryFilters(["name"])` | Same | `IDataFilter.Disable<ISoftDelete>()` scope | — | `QueryOptions.WithDeleted`; tenant and other named filters keep applying |
| Audit timestamps | You write an interceptor | You write it | Automatic | — | Automatic (`CreatedDate`, `ModifiedDate`, `DeletedDate`) |
| Audit user ids | You write it | You write it | Automatic (`CreatorId`, `LastModifierId`, `DeleterId`) | — | Properties exist; you set them |
| Domain events | — | You write the dispatch | Local events run inside the transaction, before commit; a failing handler rolls back | — | Dispatched after commit; a failing handler does not undo the saved data. The events are retried by the next save in the same request (at least once); if the request ends first they are lost — no outbox |
| Outbox / inbox | — | You write it | Built in for distributed events (EF Core, MongoDB) | — | — |
| Repository and unit of work | `DbContext` | You write it | Built in (interceptor-based UoW) | `RepositoryBase` for EF Core | `IUnitOfWork.Repository<T, TId>()` |
| Reusable query specifications | — | You write it | `Specification<T>` with `And` / `Or` / `Not`, used by repositories | Its core feature, with an evaluator | `Specification<T>` with criteria, includes, order and paging, run by `ListAsync` / `CountAsync` extensions (4.8.0; no `And` / `Or`); dynamic queries with a property whitelist |
| Multi-tenancy | Filter with a tenant id | You write it | Built in (`IMultiTenant`) | — | — (add a named tenant filter; it coexists with soft delete) |
| Licence | MIT | Yours | LGPL-3.0 | MIT (the README announces a move to a commercial licence) | MIT |

"—" means the project does not provide it.

## Where the others are stronger

- **EF Core alone** — no extra dependency. If you only need a soft-delete filter, a named query filter and a short
  `SaveChanges` override are enough.
- **ABP Framework** — a complete application framework: user-aware auditing, multi-tenancy, and a transactional
  outbox and inbox for distributed events. Choose it when you want those from day one and accept its conventions
  (interceptors through Castle DynamicProxy, ABP modules).
- **Ardalis.Specification** — small and focused: reusable, testable query specifications on top of your own
  repositories.

## Where Kck fits

Kck targets the middle: plain ASP.NET Core and EF Core, plus the persistence behaviour teams usually hand-write and get
subtly wrong — soft delete that follows your delete rules and works next to a tenant filter, audit fields, and domain
events that are never published for data that was rolled back. See the [persistence guide](providers/persistence.md),
or create a working API in five minutes with the [getting started](getting-started/README.md) guide.
