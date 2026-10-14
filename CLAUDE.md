# Kck Modular Architecture Framework — Claude Code instructions

Loaded by Claude Code sessions (local, worktree and cloud) and useful for any AI-assisted contributor.
Human contribution rules live in [CONTRIBUTING.md](CONTRIBUTING.md); this file adds what agents tend to get wrong.

## TIER: 3

<!-- Process profile of the maintainer's Claude Code setup: PR + review gates, ADR for architecture decisions,
     extra review for security modules. Changing it is an architecture decision (ADR). Rationale:
     decisions/2026-10-14-claude-process-profile.md -->

## Project conventions

- SDK is pinned by `global.json` (`10.0.402`, `rollForward: disable`) and every workflow pins the same version —
  change them together; lock files contain the SDK-coupled `Microsoft.NET.ILLink.Tasks`. Warnings are errors.
- Central package management: package versions live only in `Directory.Packages.props`.
- NuGet audit is on and vulnerabilities fail restore. A temporary exception is allowed only through a documented,
  time-boxed `*.AuditSuppress.props` imported by the affected projects (CI prints every suppression); see
  `decisions/2026-10-14-sdk-10.0.402-imagesharp-4.md`.
- Lock files are committed (`RestorePackagesWithLockFile`) and CI restores with `--locked-mode`. After any version
  change run a solution-wide `dotnet restore --force-evaluate`, then `dotnet restore --locked-mode`; restoring a
  single project is not enough (centrally pinned transitive packages change other lock files too).
- Tests use xUnit **v2** (`2.9.3`). Do not migrate to xunit.v3 — Stryker.NET mutation runs report 0%
  (`decisions/2026-05-30-xunit-v3-deferred.md`).
- Assertions use AwesomeAssertions (`using AwesomeAssertions;`), not FluentAssertions 8+ (commercial licence,
  `decisions/2026-05-29-fluentassertions-license.md`).
- Before pushing: `dotnet build -c Release` and `dotnet test -c Release` locally; CI enforces a coverage threshold.
- Mutation testing: one Stryker config per module (`stryker-*.json`).
- Architecture decisions go to `docs/adr/` (next free number); dependency and licence decisions to `decisions/`.
- Releases: pushing a `v*.*.*` tag publishes to nuget.org, which cannot delete packages (unlist only). The publish job
  waits for approval in the `nuget` GitHub environment (ADR-0027). Never push release tags without the maintainer.
- Repository owner handle: `omerkck41`.
