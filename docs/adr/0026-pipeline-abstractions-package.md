# ADR-0026 — Kck.Pipeline.Abstractions Ayrı Paketi

**Status:** Accepted  
**Date:** 2026-05-17  
**Deciders:** @omerkck41  
**Breaking:** v3.0 (paket referansı değişir)

---

## Context

Pipeline marker interface'leri (`ICachableRequest`, `ILoggableRequest`, `ISecuredRequest`,
`ITransactionalRequest`) şu an `Kck.Core.Abstractions` içinde yaşıyor.

Sorunlar:
1. **Coupling:** Pipeline behavior'ı kullanmayan projeler (sadece persistence veya messaging kullananlar)
   yine de tüm `Kck.Core.Abstractions`'ı çekmek zorunda.
2. **Separation of Concerns:** Pipeline kavramı core abstraction değil, behavior pipeline'ına özgü.
3. **E3 hedefi:** Pipeline behavior'larını bağımsız olarak versiyonlayabilmek.

---

## Decision

`Kck.Pipeline.Abstractions` adında yeni bir NuGet paketi oluşturulur.

**Paket içeriği:**
- `ICachableRequest` — önbelleklenebilir request marker
- `ILoggableRequest` — loglama marker
- `ISecuredRequest` — yetki kontrolü marker
- `ITransactionalRequest` — transaction marker

**Kck.Core.Abstractions'daki mevcut interface'ler:**
`[Obsolete]` ile işaretlenir ve `Kck.Pipeline.Abstractions` interface'lerini extend eder.
Bu sayede v2.x kodu v3.0'da derleme uyarısıyla çalışmaya devam eder.

```csharp
// Kck.Core.Abstractions/Pipeline/ICachableRequest.cs — v3.0
[Obsolete("Use Kck.Pipeline.Abstractions.ICachableRequest. Will be removed in v3.1.")]
public interface ICachableRequest : Kck.Pipeline.Abstractions.ICachableRequest { }
```

**Provider güncellemeleri:**
`Kck.Pipeline.MediatR` ve `Kck.Pipeline.Mediator`, `Kck.Pipeline.Abstractions`'a doğrudan
referans alır; `Kck.Core.Abstractions.Pipeline` namespace'inden gelen eski tipleri bırakır.

---

## Geçiş Adımları (kullanıcılar için)

```xml
<!-- Ekle -->
<PackageReference Include="Kck.Pipeline.Abstractions" Version="3.0.0" />

<!-- Değiştir -->
using Kck.Core.Abstractions.Pipeline; // eski
using Kck.Pipeline.Abstractions;      // yeni
```

v3.1'de `Kck.Core.Abstractions.Pipeline` namespace'indeki tipler tamamen kaldırılır.

---

## Consequences

**Pozitif:**
- Minimal bağımlılık: sadece pipeline behavior kullanan projeler paketi ekler.
- Bağımsız versiyonlama mümkün olur.
- `Kck.Core.Abstractions` daha saf bir abstraction kütüphanesi haline gelir.

**Negatif:**
- Kullanıcılar ekstra paket eklemeli.
- `[Obsolete]` uyarıları v3.0'da mevcut kodda görünür (derleme uyarısı, hata değil).

---

## İlgili ADR'lar

- ADR-0021: MediatR deprecation
- ADR-0022: DI Lifetime Strategy
