# Migration Guides

This directory contains migration guides that help you move between **major**
versions of the OmerkckArchitecture (`Kck.*`) packages.

## File Naming

`v<NEXT-1>.x-to-v<NEXT>.0.md`

Examples:
- `v0.x-to-v1.0.md` — moving from the 0.x series to 1.0
- `v1.x-to-v2.0.md` — moving from the 1.x series to 2.0

## When to Write One

- **MAJOR bump:** required (after 1.0).
- **0.x period:** listing each minor breaking change under `### Breaking` in
  the CHANGELOG is enough — no separate guide is needed.

Details: [`docs/policies/versioning.md`](../policies/versioning.md)

## Template

To start a new guide, copy [`_template.md`](_template.md).

## Available Guides

- [v3-to-v4.md](v3-to-v4.md) — 3.x to 4.0 (ADR-0028)
- [mediatR-to-mediator.md](mediatR-to-mediator.md) — `Kck.Pipeline.MediatR` → `Kck.Pipeline.Mediator`
- [entity-hierarchy-v3.md](entity-hierarchy-v3.md) — `Entity<TId>` → `FullEntity<TId>` (3.0)

## Automated Migration

Changes that tools such as Roslynator, `dotnet format` or IDE quick fixes can
convert automatically are listed in the **"Automated Migration"** section at
the end of each guide.

## References

- [`docs/policies/versioning.md`](../policies/versioning.md)
- [`docs/policies/deprecation.md`](../policies/deprecation.md)
- [.NET Library Guidance: Versioning](https://learn.microsoft.com/dotnet/standard/library-guidance/versioning)
