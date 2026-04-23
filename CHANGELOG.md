# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `FtpConnectionPool` for `Kck.FileStorage.FluentFtp` — pooled `AsyncFtpClient` connections with configurable `PoolSize`.
- `QueryOptions` readonly record struct in `Kck.Persistence.Abstractions` — opt-in replacement for positional `bool withDeleted / enableTracking` flags.
- `Paginate<T>.Create` static factory that replaces the removed internal synchronous constructor.
- `AddKckJob<TJob>()` helper for Hangfire and Quartz providers.
- `Kck.Exceptions.Abstractions` package split out from `Kck.Exceptions`.
- ADRs 0001–0009 under `docs/adr/`.

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
