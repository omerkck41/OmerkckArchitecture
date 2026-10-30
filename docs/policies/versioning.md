# Versioning Policy (SemVer)

This document defines the version numbering and breaking change policy of the
OmerkckArchitecture (`Kck.*`) packages.

## General Rule — [SemVer 2.0.0](https://semver.org/)

`MAJOR.MINOR.PATCH`

| Version bump | Meaning |
|---|---|
| MAJOR | Breaking change in the public API (removed / changed-signature API) |
| MINOR | Additive change to the public API (new type, new method, new feature) |
| PATCH | Bug fix, internal refactor, documentation, performance improvement |

The library is versioned automatically from git tags with `MinVer`. The tag format is `vX.Y.Z`.

## Definition of "Public API"

All of the following are in the scope of the public API:

- **All** types and members marked with the `public` modifier and located under the `Kck.*` namespace:
  - class, struct, record, interface, enum, delegate
  - method, property, field, event, constructor
  - generic constraint, default interface implementation
- `protected` and `protected internal` members **count as public** (visibility to a derived type is enough).
- `internal` members do not count as public (however, exposing them to an outside package with `[InternalsVisibleTo]` can be design debt — avoid it).

## Definition of Breaking Change

Any of the following changes counts as **breaking**:

- Removing a public type/member
- Changing a public member's signature (parameter type, count, return type)
- Removing / changing a default value
- Adding / tightening a generic constraint
- Changing the inheritance hierarchy (base class swap)
- Narrowing the access level (`public` → `internal`, `protected` → `private`)
- Changing the behavioral contract (for example, an API that returned `null` now throws an exception)

## Policy

### Since 1.0 (current major: 4.x)

- **MINOR can only be additive** to the public API. A minor may change behaviour to fix a defect; such changes are
  listed under `### Changed — behaviour` in `CHANGELOG.md`.
- Breaking changes are made only in **MAJOR**.
- Before a breaking change, `[Obsolete]` gives a warning for at least **two minor** releases
  (deprecation policy: N+0.x and N+1.x).
- Breaking changes are written in the `docs/migrations/v(N-1).x-to-vN.0.md` document (for example
  [v3-to-v4](../migrations/v3-to-v4.md)).

Before 1.0 (0.x), breaking changes were allowed in minor bumps with a CHANGELOG entry.

## Tracking with PublicApiAnalyzers

All `src/Abstractions/*` projects use `Microsoft.CodeAnalysis.PublicApiAnalyzers`.
In each abstraction project:

- `PublicAPI.Shipped.txt` — all current public API that has shipped
- `PublicAPI.Unshipped.txt` — API to be added/removed in the next release

Adding a new public API causes a build error (RS0016). Adding a line to
`PublicAPI.Unshipped.txt` in the PR is required.

During a release, the contents of `PublicAPI.Unshipped.txt` are moved to the
`PublicAPI.Shipped.txt` file, and `Unshipped` is left empty.

## CHANGELOG Discipline

Before each release, the `[Unreleased]` section of `CHANGELOG.md` is organized under these 6 headings
([Keep a Changelog](https://keepachangelog.com/en/1.1.0/)):

- `### Added` (new feature / API)
- `### Changed` (change to existing behavior)
- `### Deprecated` (will be removed in a future major release)
- `### Removed` (no longer exists)
- `### Fixed` (bug fix)
- `### Security` (security fix)

Breaking entries are marked in the heading (`### Removed — BREAKING`, `### Changed — BREAKING`) and require a
**MAJOR bump**.

## Adding a New Public API

1. Write the code. `dotnet build` produces output (RS0016).
2. Copy the generated line to `PublicAPI.Unshipped.txt`.
3. Write XML docs (they will become mandatory after the missing-docs phase).
4. Add a short description under `### Added` in `CHANGELOG.md`.

## Removing / Changing a Public API

1. First mark it `[Obsolete]` (a DiagnosticId is required — [deprecation.md](deprecation.md)).
2. Add a `*REMOVED*` line to `PublicAPI.Unshipped.txt` (PublicApiAnalyzers convention).
3. Write it under `### Deprecated` in `CHANGELOG.md`.
4. Remove it in a major release, after it has been obsolete for at least two minor releases.

## References

- [SemVer 2.0.0](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
- [Roslyn PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/tree/main/src/PublicApiAnalyzers)
- [.NET Library Guidance: Breaking Changes](https://learn.microsoft.com/dotnet/standard/library-guidance/breaking-changes)
- [`docs/policies/deprecation.md`](deprecation.md)
- [`docs/policies/support.md`](support.md)
