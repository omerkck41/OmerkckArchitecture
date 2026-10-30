# Roslyn analyzer packages (plan F4, G4.1)

Date: 2026-10-30 · Status: accepted (maintainer approval 2026-10-30; harness switched to plan B, see Outcome) ·
Constitution #1–#6

## Pre-commitment (written before the research)

**Choices to make:** (1) the Roslyn reference of `src/Analyzers/Kck.Persistence.EntityFramework.Analyzers`
(netstandard2.0, shipped inside `Kck.Persistence.EntityFramework` under `analyzers/dotnet/cs`, ADR-0031); (2) the test
harness for the analyzer and its code fix.

**Candidates:** reference — `Microsoft.CodeAnalysis.CSharp` (+ `.Workspaces` for the code fix). Harness — (A)
`Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` / `.CodeFix.Testing`; (B) hand-written tests over `CSharpCompilation` +
`CompilationWithAnalyzers` and an `AdhocWorkspace`; (C) the per-framework `.XUnit` / `.MSTest` testing packages.

**Sources:** nuget.org registry API, the package nuspecs, GitHub repository signals, the SDK's own compiler, endoflife.date,
`rules/deprecated-registry.md`.

**Criteria:** exists under the exact id; MIT or Apache-2.0 (licence gate); not deprecated; no `xunit.v3` dependency
(xUnit 2.9.3 stays, `decisions/2026-05-30-xunit-v3-deferred.md`); the analyzer's Roslyn is not newer than the oldest
compiler of a supported SDK (otherwise CS8032, the analyzer is not loaded); active in the last 12 months.

## Findings (accessed 2026-10-30)

| Item | Finding | Source |
|---|---|---|
| SDK 10.0.402 compiler | `Microsoft.CodeAnalysis.dll` 5.9.0 | `C:\Program Files\dotnet\sdk\10.0.402\Roslyn\bincore` |
| `Microsoft.CodeAnalysis.CSharp` | stable 4.13.0, 4.14.0, **5.0.0** (2025-11-18, first .NET 10 SDK), 5.3.0, 5.6.0, 5.9.0; MIT; repo dotnet/roslyn | https://api.nuget.org/v3-flatcontainer/microsoft.codeanalysis.csharp/index.json, 5.0.0 nuspec |
| `Microsoft.CodeAnalysis.CSharp.Workspaces` | same versions | https://api.nuget.org/v3-flatcontainer/microsoft.codeanalysis.csharp.workspaces/index.json |
| `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` / `.CodeFix.Testing` | **1.1.4** (2026-05-22), MIT, not deprecated; depends on `Microsoft.CodeAnalysis.Analyzer.Testing` 1.1.4 (DiffPlex, Microsoft.VisualStudio.Composition, NuGet.* — no test framework, no xunit) | nuspecs under https://api.nuget.org/v3-flatcontainer/ |
| dotnet/roslyn-sdk | **archived** (2026-08-26), licence field null; README: "moved into the dotnet/roslyn repository" | `gh api repos/dotnet/roslyn-sdk` |
| dotnet/roslyn | MIT, active (push 2026-10-30); testing sources at `src/RoslynSdk/Microsoft.CodeAnalysis.Testing/` | `gh api repos/dotnet/roslyn`, code search |
| Per-framework testing packages (`.XUnit`, `.MSTest`) | marked obsolete in favour of the framework-neutral packages | nuget.org package pages |
| .NET 10 | LTS, EOL 2028-11-14 | https://endoflife.date/api/dotnet.json |
| Registry | no `Microsoft.CodeAnalysis` / Roslyn entry | `rules/deprecated-registry.md` |

## Alternatives

| Candidate | For | Against | Decision |
|---|---|---|---|
| Reference `Microsoft.CodeAnalysis.CSharp` **5.0.0** | loads in every .NET 10 SDK (10.0.1xx–4xx); Kck targets net10.0 only | no API newer than Roslyn 5.0 (none needed) | **choose** |
| Reference 5.9.0 (matches 10.0.402) | newest API | CS8032 on SDK 10.0.1xx–3xx users: the analyzer silently does not run | reject |
| Harness (A) Testing 1.1.4 | standard harness (markup `[|...|]`, code fix verification), framework-neutral, maintained in dotnet/roslyn | larger test-only dependency graph (NuGet.*, VS Composition) — must pass NuGet audit and the licence gate | **choose**, if restore passes audit |
| Harness (B) hand-written over `CSharpCompilation` | no new test dependency beyond Roslyn | re-implements diagnostic location matching and code-fix application | Plan B if (A) fails audit/licence |
| Harness (C) `.XUnit` packages | — | obsolete | reject |

## Decision (proposed)

- Analyzer project: `Microsoft.CodeAnalysis.CSharp` 5.0.0 and `Microsoft.CodeAnalysis.CSharp.Workspaces` 5.0.0
  (code fix), `PrivateAssets="all"`; `Microsoft.CodeAnalysis.Analyzers` stays the transitive 3.11.0.
- Test project: `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` 1.1.4 and `Microsoft.CodeAnalysis.CSharp.CodeFix.Testing`
  1.1.4 with xUnit 2.9.3; added to the licence gate. If restore reports a vulnerable transitive package or the licence gate
  rejects one, switch to (B) and record it here.
- G4.1 acceptance: SDK 10.0.402 loads the analyzer without CS8032 (build of a sample with the analyzer referenced).

## Outcome (2026-10-30)

- **Harness (A) rejected by its own criterion.** Restore with NuGet audit failed: `Microsoft.CodeAnalysis.Analyzer.Testing`
  1.1.4 pulls `System.Security.Cryptography.Pkcs` 9.0.6 (NU1902 GHSA-2fmv-rv2c-59x7 moderate, NU1903 GHSA-846f-f764-76mx
  high). As pre-committed, the tests use **(B)**: `CSharpCompilation.WithAnalyzers` for diagnostics and an
  `AdhocWorkspace` + `CodeFixProvider` for the code fix (`tests/Kck.Persistence.EntityFramework.Analyzers.Tests/Verify.cs`).
  The testing packages are not referenced; no version was pinned to work around the advisory. (The code fix and its
  workspace-based test were later removed, see below.)
- **Licence gate.** The netstandard2.0 analyzer projects bring the implicit `NETStandard.Library` 2.0.3 and its
  `Microsoft.NETCore.Platforms` 1.1.0, both declared by licence URL. Reviewed as MIT: the first URL is the MIT text in
  dotnet/standard; the second (fwlink 329770) now resolves to https://github.com/dotnet/core/blob/main/license-information.md,
  which states that .NET library packages use the MIT license (accessed 2026-10-30). Build-time only: neither is in the
  `Kck.Persistence.EntityFramework` package (`.github/license-policy.json`).
- **No code fix, no Workspaces package (2026-10-30, maintainer decision after adversarial review, ADR-0031 D5).**
  The KCK1001 code fix and `Microsoft.CodeAnalysis.CSharp.Workspaces` were removed; only `Microsoft.CodeAnalysis.CSharp`
  5.0.0 remains (analyzer and tests).
- **CS8032 check passed:** a sample built with SDK 10.0.402 and a deliberate direct commit reported `error KCK1001`
  (analyzer loaded; warnings are errors).
