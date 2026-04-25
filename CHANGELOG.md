# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added (LS-FAZ-2)
- Multi-target build: `Kck.*.Abstractions` + saf provider'lar artik `net8.0` LTS + `net10.0` STS hedefli (ADR-0011). Net10-only opt-out paketler: `Kck.Bundle.WebApi`, `Kck.Persistence.EntityFramework`, `Kck.AspNetCore`, `Kck.Security.Jwt`, `Kck.Caching.Redis`, `Kck.Http.Resilience`, `Kck.Exceptions.AspNetCore`.
- `docs/adr/0011-multi-target-net8-net10.md` — multi-target karari ve net8 fallback tablosu.
- `docs/adr/0012-remove-automapper-package.md` — kullanilmayan AutoMapper paketinin kaldirilmasi karari.
- README "Quick Start" 3-sekme: WebApi (Bundle), MinimalApi (slim), WorkerService (no HTTP).
- README "Why Kck?" karsilastirma bolumu (vs ABP / FastEndpoints / Aspire).
- README Background Jobs ipucu: Hangfire ya da Quartz, ikisi birden onerilmez.

### Changed (LS-FAZ-2)
- `Microsoft.IdentityModel.JsonWebTokens 8.17.0` pin'i yorumla netlestirildi — diamond-dependency hizalamasi (Library Strategy §2.3 madde 2).
- Net8 fallback'leri eklendi (`#if NET9_0_OR_GREATER`): `IntegrationEvent.Id`, `QuartzJobScheduler.NewJobId()`, `TotpMfaProvider` secret hash. Net8'de `Guid.NewGuid()` ve `Convert.ToHexString().ToLowerInvariant()` kullanilir; net9+ surumlerde sirasiyla `Guid.CreateVersion7()` ve `Convert.ToHexStringLower()`.

### Removed (LS-FAZ-2)
- `AutoMapper 16.1.1` PackageVersion girisi `Directory.Packages.props`'tan kaldirildi (sifir kullanim, ADR-0012).

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
