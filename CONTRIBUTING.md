# Contributing to Kck Architecture

Thanks for your interest. This project welcomes issues, PRs, and discussion.

## Ground rules

- **.NET 10 SDK** required.
- Treat all analyzer warnings as errors (`TreatWarningsAsErrors=true`).
- Prefer adding tests with every code change. `dotnet test -c Release` must pass before you open a PR.

## Branching

- `main` — released, protected, no direct pushes.
- `develop` — default integration branch.
- `feat/<short-name>` — new feature or provider.
- `fix/<issue-id>` — bug fix.
- `chore/<short-name>` — infra, docs, CI.

## Commit messages — Conventional Commits

```
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

Examples:

```
feat(caching): add Redis distributed lock helper
fix(jwt): reject tokens whose kid is unknown
refactor(persistence): extract QueryOptions value object
chore(deps): bump MailKit to 4.16.0
```

Valid `type` values: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`, `security`.

Subject ≤ 72 chars. Body wraps at 100.

Breaking changes include `!` after scope **and** a `BREAKING CHANGE:` footer:

```
feat(persistence)!: switch IOptions<T> to IOptionsMonitor<T>

BREAKING CHANGE: public constructors of provider services now accept
IOptionsMonitor<T> instead of IOptions<T>. Migration: see ADR-0004.
```

## Pull requests

- One PR per logical change.
- Reference the relevant ADR or issue in the description.
- Ensure `dotnet build -c Release` shows **0 errors**.
- Update [CHANGELOG.md](CHANGELOG.md) under `[Unreleased]`.
- Update or add ADR under `docs/adr/` for architectural decisions.

### PR checklist

- [ ] Tests added or updated
- [ ] `dotnet build -c Release` passes (0 errors)
- [ ] `dotnet test -c Release --no-build` passes
- [ ] CHANGELOG updated
- [ ] Breaking changes documented in the PR body

## Security

Do **not** open public issues for security vulnerabilities. Email the maintainer
listed in [CODEOWNERS](CODEOWNERS).

## Code style

- `.editorconfig` is authoritative — no per-file overrides.
- Abstractions project stays provider-free; never reference a concrete SDK from an `*.Abstractions` project.
- Use `TryAddSingleton` / `TryAdd*` in DI extensions so consumers can override.
- Follow the conventions in `CLAUDE.md` and project-specific rules if present.
