# Deprecation Policy

This document defines the deprecation lifecycle of the public APIs of
OmerkckArchitecture (Kck.\*).

## General Rules

1. **Public API**: All types, methods, properties, fields, events,
   delegates, and interface members that are marked with the `public` modifier
   and whose namespace is under `Kck.*`.
2. **Deprecation period**: From the moment a public member is marked with
   `[Obsolete]`, it must stay in the source for **at least two minor releases**
   (N+0.x and N+1.x). It can be removed at a major bump (N → N+1.0).
3. **DiagnosticId is required**: Every `[Obsolete]` attribute must have a
   `DiagnosticId` and a `UrlFormat`.

## DiagnosticId Numbering

| Prefix         | Category                          |
| -------------- | --------------------------------- |
| `KCK0001-0099` | General API                       |
| `KCK0100-0199` | Persistence                       |
| `KCK0200-0299` | Caching                           |
| `KCK0300-0399` | Security/Auth                     |
| `KCK0400-0499` | Pipeline (MediatR/Mediator)       |
| `KCK0500-0599` | Observability                     |
| `KCK0600-0699` | Messaging/Email                   |
| `KCK0700-0799` | Background Jobs                   |
| `KCK0800-0899` | EventBus                          |
| `KCK0900-0999` | Localization/Documents/Other      |

## Attribute Template

```csharp
[Obsolete(
    "Description: WHY it is obsolete + the ALTERNATIVE API. " +
    "Removed in vX.Y.",
    DiagnosticId = "KCKnnnn",
    UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
public class LegacyService { }
```

## Lifecycle

```
v0.X.0 — API is introduced
   ...
v0.Y.0 — API is marked with @[Obsolete] (a DiagnosticId is given)
v0.Y+1.0 — API is still in the source (deprecation period 1)
v1.0.0 — API can be removed (major bump)
```

## Current Deprecated API List

| DiagnosticId | API                                              | Marked | To Be Removed |
| ------------ | ------------------------------------------------ | ----------- | ------------ |
| KCK0001      | `ITokenService.GetClaimsFromToken(string)` + `JwtTokenService.GetClaimsFromToken(string)` — does not verify the signature | 0.x | **removed: v4.0** |
| KCK0100      | `IReadRepository<T>`: `GetAsync`, `GetListAsync`, `GetListByDynamicAsync`, `AnyAsync`, `GetByIdAsync`, `CountAsync` (old signatures with bool flags) | 2.x | **removed: v4.0** (the `QueryOptions` methods became members of `IReadRepository`) |
| KCK0200      | `Kck.Pipeline.MediatR` package (`KckPipelineBuilder`, `AddKckPipeline`) — ADR-0021; MediatR 13+ commercial license | 2.x (2026-05-07) | **removed: v4.0** (the package is no longer published; 3.x is deprecated on nuget.org) |
| KCK0101      | `IQuery<T>.Query(filter, orderBy, bool withDeleted, bool enableTracking)` + `EfRepository.Query(...)` same signature — replaced by `Query(QueryOptions options, filter, orderBy)` | 4.3.0 (2026-10-29) | v5.0 (earliest; 4.4.x first) |
| KCK0002      | Bundle settings that were never read: `KckWebApiDefaults.Persistence`, `.Caching`, `.Observability`, `SecurityDefaults.Hashing`, `JwtDefaults.Algorithm` (WebApi, MinimalApi), `KckWorkerServiceDefaults.Observability` — configure those providers in code | 4.7.0 | v5.0 (earliest) |
| KCK0300      | `RsaKeySource.SecretsManager` (`Kck.Security.Jwt`) — never implemented; options validation rejects it. Set `RsaKeyBase64` | 4.7.0 | v5.0 (earliest) |
| KCK0700      | `HangfireOptions.ConnectionString` — never read; use `HangfireOptions.ConfigureStorage` | 4.7.0 | v5.0 (earliest) |
| KCK0900      | `ResilienceOptions.CircuitBreakerFailureThreshold` (`Kck.Http.Abstractions`) — never applied; use `CircuitBreakerFailureRatio` / `CircuitBreakerMinimumThroughput` | 4.7.0 | v5.0 (earliest) |
| KCK0600      | `IEmailService` (`Kck.Messaging.Abstractions`) — never implemented; every provider registers `IEmailProvider` | 4.8.0 | v5.0 (earliest) |
| KCK0901      | `IFeatureEvaluator` (`Kck.FeatureFlags.Abstractions`) — never called; use `IFeatureFlagService.IsEnabledAsync(name, context)` | 4.8.0 | v5.0 (earliest) |
| (none)       | `Kck.EventBus.InMemory` old registration methods (`AddKckEventBusInMemory` x2) — marked without a DiagnosticId | 2.x | **removed: v4.0** |

> Note (2026-10-26): By number range KCK0200 falls in the Caching category, but it was used for Pipeline. Changing the
> identity would break consumers' warning suppressions, so the number is kept. The Core.Abstractions pipeline forwarding types
> (ADR-0026) were removed in v3.2 (a minor release), contrary to this policy. From now on, removal is done only in a major release.

## Adding a New Deprecation

1. Add the `[Obsolete]` attribute according to the template.
2. Add a table row to this file.
3. Write it under `### Deprecated` in the `[Unreleased]` section of CHANGELOG.md.
4. If it does not conflict with ADR-0010 (deprecation policy), treat it as approved.

## Reference

- [.NET Deprecation guidance](https://learn.microsoft.com/dotnet/standard/library-guidance/breaking-changes)
- [Roslyn ObsoleteAttribute DiagnosticId](https://learn.microsoft.com/dotnet/api/system.obsoleteattribute.diagnosticid)
