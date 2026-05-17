# ADR-0024 — Entity<TId> Katmanlı Hiyerarşi

**Status:** Accepted  
**Date:** 2026-05-17  
**Deciders:** @omerkck41  
**Breaking:** v3.0

---

## Context

`Entity<TId>` şu an üç concern'i birden implement ediyor:
`IEntity<TId>` (kimlik) + `IAuditable` (audit trail) + `ISoftDeletable` (yumuşak silme).

Bu tasarım ISP (Interface Segregation Principle)'i ihlal ediyor:
- Kimlik taşıyan ama audit gerekmeyenbir value object türü mevcut tasarımda imkânsız.
- Soft-delete istemeden sadece audit isteyen bir entity türü yapılamıyor.
- Tüm entity'ler gereksiz yere `IsDeleted`, `CreatedBy` vb. colonlara sahip oluyor.

---

## Decision

Üç katmanlı hiyerarşi:

```
Entity<TId>
  └── AuditableEntity<TId>
        └── FullEntity<TId>
```

| Sınıf | Implement | İçerik |
|---|---|---|
| `Entity<TId>` | `IEntity<TId>` | Id, DomainEvents |
| `AuditableEntity<TId>` | `Entity<TId>`, `IAuditable` | + CreatedBy/Date, ModifiedBy/Date |
| `FullEntity<TId>` | `AuditableEntity<TId>`, `ISoftDeletable` | + IsDeleted, DeletedBy/Date, RowVersion |

**Geçiş tavsiyesi:**
- Eski: `class Product : Entity<Guid>` → Yeni: `class Product : FullEntity<Guid>` (en yaygın durum)
- Yalnızca audit isteyen: `class LogEntry : AuditableEntity<Guid>`
- Yalnızca kimlik: `class ValueObject : Entity<int>`

---

## AuditInterceptor Etkisi

`AuditInterceptor`, `entry.Entity is IAuditable` / `is ISoftDeletable` pattern matching kullandığından
bu değişiklikten etkilenmez — mevcut kod v3.0'da değişmeden çalışır.

---

## Consequences

**Pozitif:**
- ISP uyumu — entity sınıfları yalnızca ihtiyaç duydukları concern'leri taşır.
- EF Core şema: `Entity<TId>` kullanan basit tablolar gereksiz sütunlardan kurtulur.
- Daha açıklayıcı: sınıf adı tek bakışta capability'yi gösterir.

**Negatif (breaking):**
- `Entity<TId>` extend eden her sınıf `AuditableEntity<TId>` veya `FullEntity<TId>`'ye geçmeli.
- Tek satır değişim ama her entity dosyasında gerekiyor.
- EF Core migration: sütun düzeni değişmez ama ORM konfigürasyonu `FullEntity<TId>` ile teyit edilmeli.

---

## Migration

`docs/migrations/entity-hierarchy-v3.md` — adım adım geçiş kılavuzu.

---

## İlgili ADR'lar

- ADR-0005: EF Repository Factory
- ADR-0022: DI Lifetime Strategy
