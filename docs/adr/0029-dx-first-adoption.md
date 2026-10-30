# ADR-0029 — Adoption: Developer Experience First, Persistence as the Hero Module

**Status:** Accepted  
**Date:** 2026-10-30  
**Deciders:** @omerkck41  
**Breaking:** No (4.x minor releases only)

---

## Context

The framework is technically mature: 58 packages, MIT, extensive CI (CodeQL, Sonar, mutation testing, licence gate,
Native AOT smoke), Trusted Publishing to nuget.org. Adoption is low and is not limited by code quality:

1. **First contact is weak.** `dotnet new kck-webapi` produces a single controller project with a WeatherForecast
   sample: no database, no persistence, no tests. Nothing in the first minutes shows what Kck does better than wiring
   EF Core, Mediator and Serilog by hand.
2. **Documentation is mostly Turkish** (`docs/providers/*`, FAQ, migration guides, policies) while the NuGet audience
   is global, and there is no "getting started" path, only a list of modules.
3. **Docs drift from code.** README code blocks compile in CI (H21), `docs/` blocks do not; `docs/providers/persistence.md`
   shows a dispatcher type that does not exist.
4. **The strongest module is hidden.** `Kck.Persistence.EntityFramework` implements behaviour that teams usually get
   wrong when they write it themselves: soft-delete that follows the model's delete rules across loaded dependants,
   domain events tied to the transaction commit (discarded on rollback, re-queued on failure), audit fields, DbContext
   pooling. Yet every user still has to write their own `IDomainEventDispatcher`.
5. **Baseline (2026-10-30):** see `docs/policies/adoption-metrics.md`.

---

## Decision

| # | Decision |
|---|---|
| D1 | Adoption work focuses on **developer experience**, with **`Kck.Persistence.EntityFramework` as the hero module**: the template, the getting-started path and the reference sample lead with its behaviour. Promotion work (blog posts, community outreach) is out of scope until the DX work ships. |
| D2 | **English** is the single language of user-facing documentation, templates, samples and package READMEs. ADRs from 0029 on are English; ADR-0001…0028 stay as historical records in their original language. |
| D3 | `dotnet new kck-webapi` becomes a **solution template** (API + tests) using **SQLite** by default, with `--database postgres|sqlserver`, minimal-API feature slices for an **order management** domain, no secrets, and an interactive API UI in Development (subject to a currency check). The template's output changes; generated projects are not affected. |
| D4 | Delivery is **phased 4.x minor releases** (4.6.0 template, 4.7.0 analyzers and error messages). No breaking API change; breaking ideas go to a v5 list. Documentation is published from `main` as it lands; pages for unreleased features say which version brings them. |
| D5 | Documentation follows the **Diátaxis** structure (tutorials, how-to guides, reference, explanation). Published docs are guarded in CI: no Turkish text outside historical ADRs, every C# block compiles (explicit, counted exemptions only), docfx fails on broken links. |
| D6 | Success is measured (plan §9): time from `dotnet new` to a healthy endpoint (target ≤ 60 s, warm cache), generated solution builds with 0 warnings for every database choice, the docs guards, zero analyzer false positives in the repository, and adoption metrics recorded at baseline, +30 and +60 days. |

Implementation plan, acceptance examples, premortem and rollback: `docs/plans/2026-10-30-dx-hero-module-plan.md`.

---

## Alternatives considered

- **Hero module alone (no DX work):** a sharp story, but the first `dotnet new` would still show a WeatherForecast
  project and Turkish docs. Rejected as the first step; kept as the focus of the DX work.
- **Promotion first (comparison posts, community):** brings visitors to a first experience that does not convert.
  Deferred until D1 ships.
- **`Kck.Pipeline.Mediator` as the hero:** strong demand since MediatR became commercial, but well-served by existing
  free libraries; the persistence behaviour is more distinctive.
- **English and Turkish docs side by side:** roughly doubles maintenance; translations drift. Rejected.
- **Keep the old template, add a new one:** two entry points with different messages. Rejected; the old output stays
  available by installing an older `Kck.Templates` version.
- **A single big release at the end:** feedback arrives late. Rejected for phased minors.

---

## Consequences

- (+) The first experience shows the hero module's behaviour within a minute, with tests.
- (+) Docs cannot silently drift from code or fall back to Turkish.
- (+) Domain event handling no longer requires user-written plumbing (4.6.0, ADR-0030).
- (−) Translating ~2,200 lines of docs is the largest single cost; it is done page by page behind a per-file guard.
- (−) Users scripting the old template output see a different tree after 4.6.0 (documented in the release notes).
- (−) Historical ADRs remain in Turkish.

---

## Related

- ADR-0030 (domain event handlers), ADR-0031 (analyzer packaging) — to be written in their phases
- `docs/plans/2026-10-30-dx-hero-module-plan.md`
- `docs/plans/2026-10-26-dis-inceleme-yol-haritasi.md` (decision D5: DX focus)
