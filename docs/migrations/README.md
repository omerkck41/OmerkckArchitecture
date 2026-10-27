# Migration Rehberleri

Bu dizin OmerkckArchitecture (`Kck.*`) paketleri arasindaki **major**
surum gecislerinde kullanicilara yol gosteren migration dokumanlarini
icerir.

## Dosya Adlandirmasi

`v<NEXT-1>.x-to-v<NEXT>.0.md`

Ornek:
- `v0.x-to-v1.0.md` — 0.x serisinden 1.0'a gecis
- `v1.x-to-v2.0.md` — 1.x serisinden 2.0'a gecis

## Ne Zaman Yazilir

- **MAJOR bump:** zorunlu (1.0 sonrasi).
- **0.x donemi:** her minor breaking change CHANGELOG `### Breaking` altinda
  yeterli — ayri rehber yazmaya gerek yok.

Detay: [`docs/policies/versioning.md`](../policies/versioning.md)

## Sablon

Yeni rehber baslatmak icin: [`_template.md`](_template.md) kopyala.

## Mevcut Rehberler

- [v3-to-v4.md](v3-to-v4.md) — 3.x'ten 4.0'a (ADR-0028)
- [mediatR-to-mediator.md](mediatR-to-mediator.md) — `Kck.Pipeline.MediatR` → `Kck.Pipeline.Mediator`
- [entity-hierarchy-v3.md](entity-hierarchy-v3.md) — `Entity<TId>` → `FullEntity<TId>` (3.0)

## Otomatik Migration

Roslynator, `dotnet format`, IDE quick fix gibi araclarla otomatik
donusum mumkun olan degisiklikler her rehberin sonunda **"Otomatik
Migration"** bolumunde belirtilir.

## Referans

- [`docs/policies/versioning.md`](../policies/versioning.md)
- [`docs/policies/deprecation.md`](../policies/deprecation.md)
- [.NET Library Guidance: Versioning](https://learn.microsoft.com/dotnet/standard/library-guidance/versioning)
