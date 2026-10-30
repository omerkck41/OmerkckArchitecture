# Test Coverage Policy

This document defines the coverage gate and the gradual-increase policy
for the OmerkckArchitecture (`Kck.*`) projects.

## General Rule — Regression Prevention

**No PR may lower coverage.** The coverage gate (the `Enforce coverage threshold`
step in `build-test.yml`) enforces this. The threshold values rise **gradually**
over time, but each increase depends on **writing tests**, not on a decree.

## Current Thresholds (2026-10-29, rounded; build-test.yml)

| Metric | Threshold | CI measurement (3 consecutive green builds, 2026-10-29) |
|---|---|---|
| Line | **70%** | 72.9% / 73.0% / 73.3% |
| Branch | **67%** | 70.1% / 70.1% / 70.1% |

> **H20 (2026-10-29, #222):** Unit tests for Observability.OpenTelemetry, Http.Resilience, AspNetCore, Logging.Serilog, and
> the abstractions. The threshold is about 3 points below the measurement (absorbs the local / CI difference). Previous threshold: 55/51.

> Local measurement: produced with `dotnet test --collect:"XPlat Code Coverage"` +
> `reportgenerator -reporttypes:TextSummary` (the same command as CI).

## Gradual Increase Roadmap

The source report (`tasks/library-strategy-2026-04-25.md` Section 7.1) summarizes
the industry standard as follows:

- Library project: 80%+ line, 60%+ branch (Polly, Serilog, EF Core ~85%)
- Provider/wrapper: 70%+ line

Roadmap (each stage is a separate phase and requires writing tests):

| Stage | Target Line | Target Branch | Phase / Trigger |
|---|---|---|---|
| **0** | 40 | 35 | LS-FAZ-4 — regression prevention |
| **1** ✅ | 50 | 45 | Faz-B2 — Entity/Paginate/Error/property-based tests |
| **2** ✅ | 65 | 50 | H20 — provider and abstraction unit tests (2026-10-29, threshold 70/67) |
| **3** | 75 | 60 | Filling the gaps with mutation testing (Stryker.NET) |

## Preconditions for Raising a Stage

A threshold **cannot change** until the **test writing** up to it is complete:

1. Local measurement: the `reportgenerator -reporttypes:TextSummary` output must be above the new target.
2. At least 3 consecutive green builds in CI — a lasting state, not a temporary deviation.
3. The PR description lists which tests were added.
4. The threshold change and the reason are made in the same PR.

## Measurement Reproducibility

Coverage measurement can be sensitive to the host SDK and the test discovery method.
For consistent measurement:

```bash
dotnet test OmerkckArchitecture.sln --no-build \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

dotnet tool restore
dotnet reportgenerator \
  -reports:'**/coverage.cobertura.xml' \
  -targetdir:./coverage-report \
  -reporttypes:'TextSummary'

cat ./coverage-report/Summary.txt
```

The CI command (`build-test.yml`) runs the same steps.

## Excluded Projects

Do **not** collect coverage measurement in these test projects:

- `tests/**/*.Tests.csproj` — ✓ included
- `tests/Kck.Benchmarks` — ✗ excluded (not a test project, a console app)
- `samples/**/*.csproj` — ✗ excluded (application code, not framework code)

The exclusion pattern is not explicit in `coverlet.runsettings` or `dotnet test --filter`.
Currently the `XPlat Code Coverage` collector gathers coverage for all referenced
projects. The benchmark/sample csproj files do not reference a test asset, so they are
not included automatically.

## References

- `.github/workflows/build-test.yml` (Enforce coverage threshold step)
- `tasks/library-strategy-2026-04-25.md` Section 7.1
- [Coverlet documentation](https://github.com/coverlet-coverage/coverlet)
- [reportgenerator](https://github.com/danielpalme/ReportGenerator)
