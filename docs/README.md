# Kck documentation

Kck is a modular .NET 10 framework: **Abstractions** declare contracts, **Providers** implement them against concrete
technologies, **Bundles** compose defaults for a hosting model. Its persistence module adds what teams usually
hand-write around EF Core: soft delete that follows your delete rules, audit fields, and domain events dispatched after
the transaction commits.

## Get started

- [Your first Kck API in five minutes](getting-started/README.md) — `dotnet new kck-webapi`, a token, an order, a
  rollback that discards its event, a soft delete, the tests.

## Reference

- [Provider guides](providers/README.md) — one guide per area (persistence, caching, security, messaging, ...)
- [Persistence: Kck compared](persistence-comparison.md) — with EF Core alone, a hand-written layer, ABP and
  Ardalis.Specification
- [Architecture decision records](adr/README.md) — why the framework is built the way it is
- [Configuration schema](configuration-schema.md) — the `Kck:*` configuration keys
- API reference — generated from the source, in the site's top navigation

## Policies and upgrades

- [Versioning](policies/versioning.md), [deprecation](policies/deprecation.md), [support](policies/support.md)
- [Migration guides](migrations/README.md) — v3 → v4 and earlier
- [Licensing](licensing.md) — dependency licence policy
- [Test strategy](test-strategy.md) and [coverage policy](policies/test-coverage.md)
- [FAQ](faq.md)

## Contributing

[CONTRIBUTING.md](../CONTRIBUTING.md) — build, test, and the rules for pull requests. Documentation is written in
English; `.github/docs-english.txt` lists the pages already checked in CI.
