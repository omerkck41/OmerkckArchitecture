# Test Strategy

This document defines the test pyramid of OmerkckArchitecture (`Kck.*`) and when
to use each test category.

## Test Pyramid

```
            /\
           /  \    Integration tests (Testcontainers)
          /----\   - real DB / broker / external service
         /      \  - LS-FAZ-4 PoC: Kck.Persistence.EntityFramework.Tests
        /        \
       /----------\ Unit tests (xUnit + NSubstitute + InMemory)
      /            \ - all providers + abstractions
     /              \ - default test collection
    /----------------\
```

## Unit Tests

**Purpose:** Quickly verify the behavior of a type or method.

**Tools:**
- xUnit `[Fact]` / `[Theory]`
- `FluentAssertions` (assertion chains)
- `NSubstitute` (interface mocks)
- EF Core: `Microsoft.EntityFrameworkCore.InMemory` provider

**Location:** `tests/Kck.<Project>.Tests/`

**Filter:** Included in default test discovery. No special filter is needed.

**Trait:** None (default).

**CI:** Runs on both the ubuntu and windows runners.

## Integration Tests (Testcontainers)

**Purpose:** Verify real behavior that mocks/InMemory cannot catch:
- Concurrency tokens (xmin, RowVersion)
- Transaction isolation level
- JSON column mappings
- Provider-specific query translation
- Connection pool behavior
- Network/IO timeout patterns

**Tools:**
- `Testcontainers.PostgreSql`, `Testcontainers.Redis`, `Testcontainers.RabbitMq`, and so on
- The xUnit `IAsyncLifetime` pattern

**Location:** Inside the existing test project, in a separate test class:
`<Project>.Tests/<Service>IntegrationTests.cs`

**Trait:** `[Trait("Category", "Integration")]` (at class or method level)

**Filter:**
- Unit test step: `--filter "Category!=Integration"`
- Integration test step: `--filter "Category=Integration"` (ubuntu only)

**CI:** Only on the **ubuntu-latest** runner. Docker is not pre-installed on the
Windows GitHub-hosted runner, so integration tests are skipped there.

**Running locally:** Docker Desktop must be running.

```bash
# Unit tests only (no Docker):
dotnet test --filter "Category!=Integration"

# Integration tests only (Docker required):
dotnet test --filter "Category=Integration"
```

## Current Integration Test Coverage (LS-FAZ-4)

| Test Project | Container | Status | Phase |
|---|---|---|---|
| `Kck.Persistence.EntityFramework.Tests` | `postgres:16-alpine` | **PoC** (LS-FAZ-4) | ✓ |
| `Kck.Caching.Redis.Tests` | (mock — InMemory-like) | Awaiting rollout | LS-FAZ-4.5 |
| `Kck.EventBus.RabbitMq.Tests` | (mock) | Awaiting rollout | LS-FAZ-4.5 |
| `Kck.EventBus.AzureServiceBus.Tests` | (mock) | Awaiting rollout | LS-FAZ-4.5 |
| `Kck.Search.Elasticsearch.Tests` | (mock) | Awaiting rollout | LS-FAZ-4.5 |
| `Kck.Security.Secrets.AzureKeyVault.Tests` | (mock) | Awaiting rollout | LS-FAZ-4.5 |

LS-FAZ-4.5 aims to roll the PoC pattern out to the other 5 test projects.

## Benchmarks (BenchmarkDotNet)

**Purpose:** Catch performance regressions with micro-benchmarks. This is a profiling
tool, **not** a test.

**Location:** `tests/Kck.Benchmarks/`

**Running:**

```bash
dotnet run -c Release --project tests/Kck.Benchmarks
dotnet run -c Release --project tests/Kck.Benchmarks -- --filter "*Paginate*"
```

**CI:** Benchmarks do not run in CI (benchmark results are not deterministic).
CI regression detection (bencher.dev or a benchmark GitHub action) may be added in the future.

## Mutation Testing (Stryker.NET) — Not Yet Available

Section 7.4: start a mutation score on critical abstractions (Result, Paginate, Filter) —
target >70%. Marked as **trial mode**, not yet implemented.

## Property-Based Testing — Not Yet Available

Section 7.3: property tests with FsCheck/CsCheck for `Paginate.Create` math, the `Filter`
parser, and `PathSanitizer`. Marked as **trial mode**.

## Coverage Policy

Details: [`docs/policies/test-coverage.md`](policies/test-coverage.md)

- Current threshold: **line 40% / branch 35%** (regression prevention)
- Roadmap: gradual increases, which require writing tests

## References

- [`docs/policies/test-coverage.md`](policies/test-coverage.md)
- `tasks/library-strategy-2026-04-25.md` Sections 5.7, 7.1, 7.5
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
