# Diagnostics

Analyzers shipped inside Kck packages report these diagnostics at build time (ADR-0031). Ids `KCK1xxx` are analyzer
rules; `KCK0xxx` ids mark obsolete APIs (see [deprecation policy](../../policies/deprecation.md)).

| Id | Severity | Package | Title |
|---|---|---|---|
| [KCK1001](KCK1001.md) | Warning | `Kck.Persistence.EntityFramework` | Commit through IUnitOfWork so domain events are dispatched |
| [KCK1002](KCK1002.md) | Info | `Kck.Persistence.EntityFramework` | Prefer SaveChangesAsync |

Available from 4.7.0.
