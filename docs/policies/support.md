# Support Policy (.NET Version Matrix)

This document defines the target framework support model of the
OmerkckArchitecture (`Kck.*`) packages.

## General Rule

The library follows Microsoft's .NET support policy:

- **LTS** (Long Term Support, 3 years): the library supports it **for its entire lifecycle**.
- **STS** (Standard Term Support, 24 months since .NET 9): the library targets it **for the duration of its support
  window**.
- As the end of an LTS period approaches, the old LTS is removed gradually once the new LTS is released.

## Current Support Matrix (as of 2026-10-30)

| TFM | Type | Microsoft End of Support | Library Status |
|---|---|---|---|
| `net8.0` | LTS | 2026-11-10 | **Removed** (ADR-0019, v2.0) |
| `net10.0` | LTS | 2028-11-14 | **Only target** |

Source: [endoflife.date/dotnet](https://endoflife.date/dotnet) (accessed 2026-10-30): .NET 10 released 2025-11-11 as
LTS, end of support 2028-11-14. The official policy is on
[Microsoft's support page](https://dotnet.microsoft.com/platform/support/policy/dotnet-core).

## Target Framework Decision

**Target framework policy (ADR-0019):**

- All packages: `net10.0` (single target)
- ADR-0011 (multi-target net8+net10) was superseded by ADR-0019.

Details: [`docs/adr/0019-drop-net8-target.md`](../adr/0019-drop-net8-target.md)

## Release Policy

- End of LTS: a warning period of at least **one minor** release (CHANGELOG `### Deprecated` → `Target framework X removal`).
- Major bump (after 1.0): TFMs are removed only in a major release.
- During 0.x: the TFM can change in minor bumps (with a CHANGELOG entry).

## Adding / Removing a TFM

1. Write an ADR (Section 11.x) — rationale + alternatives.
2. Update the default `<TargetFrameworks>` in `Directory.Build.props`.
3. Review the net10-only csproj files (are there any candidates for multi-targeting?).
4. The CI matrix (`.github/workflows/build-test.yml`) builds/tests the new TFM.
5. Add an entry under `[Unreleased]` in `CHANGELOG.md`.
6. Update the matrix in this document.

## References

- [.NET Support Policy (Microsoft)](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
- [ADR-0011: Multi-Target net8.0 + net10.0 (Superseded)](../adr/0011-multi-target-net8-net10.md)
- [ADR-0019: Dropping net8.0 support](../adr/0019-drop-net8-target.md)
- `tasks/library-strategy-2026-04-25.md` Section 6.4
