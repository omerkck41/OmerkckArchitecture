# Dokuman Dizini

Bu dizin framework'un kullanim rehberlerini, mimari kararlarini ve
gelistirici referanslarini tutar.

## Icindekiler

### Kullanim Rehberleri
- [Provider rehberleri](providers/) — 17 kategoride paket bazli kullanim

### Mimari Kararlar
- [ADR (Architecture Decision Records)](adr/) — bagBulcun teknik kararlar

## Ana Bolumler

| Konu | Link |
|---|---|
| **Provider index** | [providers/README.md](providers/README.md) |
| **ADR index** | [adr/README.md](adr/README.md) |
| **Ornekler** | [../samples/](../samples/) — WebApi, MinimalApi, WorkerService |
| **Katki** | [../CONTRIBUTING.md](../CONTRIBUTING.md) |

## Hizli Baslangic

1. [Ana README](../README.md) — framework'e genel bakis
2. [Bundle seciminiz](providers/README.md#kategoriler) — amaca uygun paket grubu
3. [Ornek projeler](../samples/) — calisir kod ornekleri

## Arastirma

Ne aradiginiz net degilse:

- **"Hangi cache secmeliyim?"** → [caching.md](providers/caching.md#secim-kriterleri)
- **"JWT nasil konfigure ederim?"** → [security.md](providers/security.md#jwt)
- **"Background job nasil yazarim?"** → [background-jobs.md](providers/background-jobs.md)
- **"Miras sistemi devraldim, nereden baslasam?"** → [persistence.md](providers/persistence.md) +
  [observability.md](providers/observability.md)
