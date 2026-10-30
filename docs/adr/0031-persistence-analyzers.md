# ADR-0031 — Roslyn Analyzers Shipped with Kck.Persistence.EntityFramework

**Status:** Accepted  
**Date:** 2026-10-30  
**Deciders:** @omerkck41  
**Breaking:** No (new diagnostics; KCK1001 is a warning, which fails builds that treat warnings as errors)

---

## Context

`Kck.Persistence.EntityFramework` dispatches domain events after a commit made through `IUnitOfWork` (ADR-0030). Two
uses of EF Core directly bypass or weaken that guarantee, and both compile and run without any sign of trouble:

- A transaction opened with `DbContext.Database.BeginTransaction(Async)` and committed with
  `IDbContextTransaction.Commit(Async)`: the events wait for the context's next `SaveChanges`; if none follows, they are
  lost without a trace (`docs/providers/persistence.md`, "Interceptors").
- Synchronous `DbContext.SaveChanges()`: event dispatch is asynchronous, so it runs on a thread-pool thread while the
  caller blocks (a warning is logged at run time).

Plan F4 (`docs/plans/2026-10-30-dx-hero-module-plan.md`, G4.1–G4.4) turns these into compile-time diagnostics.

---

## Decision

| # | Decision |
|---|---|
| D1 | A Roslyn analyzer project `src/Analyzers/Kck.Persistence.EntityFramework.Analyzers` (netstandard2.0, `IsRoslynComponent`, not packed on its own) is shipped **inside** the `Kck.Persistence.EntityFramework` package under `analyzers/dotnet/cs`. Everyone who installs the package gets it, with a matching version; `Kck.Bundle.WebApi` passes it on (its dependency on the package includes analyzers). |
| D2 | It references `Microsoft.CodeAnalysis.CSharp` **5.0.0**, the compiler of the first .NET 10 SDK, so it loads in every .NET 10 SDK (no CS8032). Tests drive Roslyn directly (`CSharpCompilation.WithAnalyzers`) with xUnit 2.9.3: the Roslyn testing packages pull a vulnerable transitive package. Sources: `decisions/2026-10-30-roslyn-analyzer-packages.md`. |
| D3 | Diagnostic ids `KCK1xxx` (KCK0xxx stays reserved for `[Obsolete]`), category `Kck.Persistence`, help link to `docs/reference/diagnostics/<id>.md` on GitHub. |
| D4 | **KCK1001** (warning): a call to `IDbContextTransaction.Commit` / `CommitAsync` in a compilation that uses domain events — it implements `IDomainEventHandler<T>` or `IDomainEventDispatcher`, or calls `AddDomainEventHandlers` / `AddDomainEventDispatch`. Without domain events nothing is reported (no noise for applications that do not use them). The check needs the whole compilation, so it runs at compilation end: it shows in builds, not while typing. Limit: a project that commits directly while its handlers and registration live only in other projects is not reported. |
| D5 | **No code fix** for KCK1001. The diagnostic page shows the manual change. A fix was built and removed after an adversarial review: moving a transaction to the unit of work changes its semantics in ways the syntax does not show — removing the `using` drops the implicit rollback on an early `return` / `continue` (another iteration then commits the rows), a `RollbackAsync` in a `catch` after a failed dispatch throws "No active transaction" and hides the real error, a transaction on a different `DbContext` (factory, second context type) is no longer around the saves, and several scopes (nested types, static lambdas, nullable unit of work) produce code that does not compile. |
| D6 | **KCK1002** (info, shown as a suggestion): a call to `DbContext.SaveChanges()` / `SaveChanges(bool)` on a context type. |
| D7 | Precision gate (G4.4): every project under `samples/` builds with the analyzer (`samples/Directory.Build.props`), and the template smoke test builds the template output against the packed packages, which carry it; warnings are errors, so a KCK1001 fails the build. Not part of the gate: the library's own tests (they commit directly on purpose) and `Kck.ReadmeSnippets`, which runs no analyzers (the diagnostic pages show the anti-pattern). |
| D8 | Users turn a rule off with `.editorconfig` (`dotnet_diagnostic.KCK1001.severity = none`) or `#pragma warning disable KCK1001`. |
| D9 | The analyzer ships with the next release (4.7.0); until then the diagnostic pages say "Available from 4.7.0" (plan R13). |

---

## Alternatives considered

- **A separate `Kck.Analyzers` package:** optional, so the users who most need it would not install it, and its version
  could drift from the library's. Rejected (plan §6).
- **Report KCK1001 for every direct commit:** applications without domain events lose nothing by committing directly;
  a warning there is noise and gets suppressed wholesale. Rejected for D4.
- **A code fix that rewrites only `Commit`:** compiles, then throws at run time because the unit of work has no
  transaction. Rejected.
- **A code fix that rewrites begin, commit and rollback together under narrow conditions:** implemented, then
  removed (D5). Each guard (control flow after the `using`, rollback in `catch` / `finally`, the context behind the
  unit of work, speculative binding) closes one case, and a miss changes what is committed without any warning. A
  later version may offer it as a refactoring that the user applies knowingly.
- **Target Roslyn 5.9.0 (SDK 10.0.402):** newer API, but older .NET 10 SDKs would not load the analyzer, silently.
  Rejected for D2.
- **Runtime-only guidance (log warnings):** already exists for synchronous saves; it is found late, if at all.
  Analyzers complement it.

---

## Consequences

- (+) The two most common ways to lose or delay domain events are visible at build time.
- (−) The change to `IUnitOfWork` is manual (D5).
- (−) KCK1001 is a warning: a project with `TreatWarningsAsErrors` that commits directly fails its build after the
  upgrade until it moves to `IUnitOfWork` or suppresses the rule (CHANGELOG, "Changed — behaviour").
- (−) Compilation-end analysis: KCK1001 is not shown live in the editor.
- (−) One more project to build; the tests carry their own small Roslyn harness.

---

## Related

- ADR-0029 (adoption plan), ADR-0030 (domain event handlers)
- `decisions/2026-10-30-roslyn-analyzer-packages.md`
- `docs/reference/diagnostics/KCK1001.md`, `KCK1002.md`
