# ADR-0032 — Package Dependency Surface: No Shared-Framework Reference Unless the Code Needs It

**Status:** Accepted (5.0)  
**Date:** 2026-11-01  
**Deciders:** @omerkck41  
**Breaking:** Yes (consumer dependency graph; no API change)

---

## Context

31 of the 36 packable projects declare `<FrameworkReference Include="Microsoft.AspNetCore.App" />`. For 26 of them the
code uses only `Microsoft.Extensions.*` abstractions (options, logging, dependency injection, hosting, caching,
configuration, health checks) that ship as NuGet packages. The framework reference flows into the published nuspec as a
`<frameworkReference>`, so every consumer of, for example, `Kck.Caching.InMemory` is forced onto the ASP.NET Core shared
framework: a `Microsoft.NET.Sdk.Worker` or console application fails to start on a `mcr.microsoft.com/dotnet/runtime`
image, and a plain class library gains a framework it never asked for (plan K5,
`docs/plans/2026-10-31-provider-saglik-ve-atil-kod-plani.md`; evidence: the nuget.org nuspec of `kck.caching.inmemory`
4.8.0).

Two providers carry the framework through their own dependency as well: `Hangfire` (meta package →
`Hangfire.AspNetCore`) and `Serilog.AspNetCore`. Removing the framework reference from those projects alone changes
nothing.

Sources and currency check: `decisions/2026-11-01-frameworkreference-to-extensions-packages.md`.

---

## Decision

| # | Decision |
|---|---|
| D1 | A packable project references `Microsoft.AspNetCore.App` only when its code uses `Microsoft.AspNetCore.*` types. Today that is `Kck.AspNetCore`, `Kck.Exceptions.AspNetCore`, `Kck.Bundle.WebApi`, `Kck.Bundle.MinimalApi` and `Kck.Observability.OpenTelemetry` (`MapKckHealthChecks`, ASP.NET Core instrumentation). |
| D2 | The other 26 projects drop the framework reference and reference the `Microsoft.Extensions.*` packages they actually use, as direct references at 10.0.14 (the line already pinned in `Directory.Packages.props`; third-party graphs may resolve older 10.0.x transitively): mostly `Options` and `Logging.Abstractions`; `DependencyInjection.Abstractions` (ClosedXml, EventBus.InMemory, UserSecrets, Hangfire, Serilog), `Caching.Memory` (InMemory cache, Totp), `Caching.Abstractions` (Mediator), `Configuration.Abstractions` (UserSecrets), `Configuration.Binder` (WorkerService bundle), `Diagnostics.HealthChecks` (Redis cache, Aspire). The smallest set that compiles; abstractions packages where only abstractions are consumed. |
| D3 | `Microsoft.Extensions.Logging.Abstractions` is referenced **directly** by every project that uses `[LoggerMessage]`: the source generator ships in the package's analyzers folder and analyzers do not flow transitively. |
| D4 | `Kck.BackgroundJobs.Hangfire` replaces `Hangfire` with `Hangfire.Core` + `Hangfire.NetCore` (1.8.25). Consumers that relied on the transitive `Hangfire.SqlServer` add it themselves (they already choose the storage through `HangfireOptions.ConfigureStorage`). `Hosting.Abstractions`, `Logging.Abstractions` and `Configuration.Abstractions` are referenced directly to lift `Hangfire.NetCore`'s 3.0.0 floor to the 10.0.14 line. |
| D5 | `Kck.Logging.Serilog` replaces `Serilog.AspNetCore` with `Serilog.Extensions.Hosting` 10.0.0 + `Serilog.Sinks.Console` 6.1.1 + `Serilog.Sinks.File` 7.0.0 (the versions `Serilog.AspNetCore` pins). Consumers that used the transitive `UseSerilogRequestLogging`, `Serilog.Settings.Configuration` or `Serilog.Sinks.Debug` add those packages themselves. |
| D6 | Per-project `PackageReference`s, not a shared `Directory.Build.props` under `src/Providers`: a shared file would hand every provider dependencies it does not need (the repository already follows this pattern in SkiaSharp, AwsS3, RabbitMq, Hybrid, EntityFramework). |
| D7 | Proof per package: `dotnet pack` output has no `<frameworkReferences>` element for the 26 projects and lists the `Microsoft.Extensions.*` dependencies instead (acceptance example 13 of the plan); `samples/Kck.AotSmoke` drops its own framework reference as a second proof; `samples/Kck.Sample.WorkerService` (`Microsoft.NET.Sdk.Worker`, Hangfire + Serilog) builds without the shared framework and its `runtimeconfig.json` lists only `Microsoft.NETCore.App`. (The sample's start-up failure in `HangfireJobScheduler.AddOrUpdateRecurringAsync` predates this change and is tracked separately.) |
| D8 | Lock files: after the package changes a solution-wide `dotnet restore --force-evaluate`, then `dotnet restore --locked-mode`. The lock diff is large on purpose: .NET 10 package pruning had removed the shared-framework packages from the lock files, and they now come back as real dependencies. |
| D9 | Known limit, out of scope here: `Kck.Bundle.WorkerService` still reaches the shared framework through `Kck.Observability.OpenTelemetry`. Splitting the ASP.NET Core part of that provider into its own package is a separate decision. |

---

## Consequences

- Worker and console consumers of the 26 packages no longer depend on the ASP.NET Core shared framework; web consumers
  see no difference (the framework already contains the referenced packages).
- The consumer dependency graph changes (framework → explicit packages). The public API does not; package validation
  against the 4.8.0 baseline passes without new suppressions.
- Two breaking transitive changes (D4, D5) are documented in `docs/migrations/v4-to-v5.md` and CHANGELOG `[Unreleased]`.
- The licence gate keeps its documented `Hangfire*` exception (LGPL-3.0 or commercial); Serilog packages are Apache-2.0.

## Alternatives considered

- **Keep the framework reference** (status quo): rejected — K5 stays unresolved.
- **Two package variants per provider** (with/without ASP.NET Core): rejected — NuGet has no per-TFM variant for the same
  framework, it doubles the package count, and D1 already separates the five packages that need the framework.
- **`Microsoft.Extensions.*` 9.0.x for broader compatibility**: rejected — the target is `net10.0`; mixing framework and
  package versions risks NU1605/CS0433, and .NET 9 reaches end of life on 2026-11-10.

## References

- `decisions/2026-11-01-frameworkreference-to-extensions-packages.md` (sources, versions, dates)
- `docs/plans/2026-10-31-provider-saglik-ve-atil-kod-plani.md` K5 / F3, decision D6
- ADR-0028 (v4 scope), ADR-0009 (TryAdd registrations), ADR-0022 (lifetimes)
