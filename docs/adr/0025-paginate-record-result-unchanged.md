# ADR-0025 — Paginate<T> record class; Result<T> değişmez

**Status:** Accepted  
**Date:** 2026-05-17  
**Deciders:** @omerkck41  
**Breaking:** v3.0 (Paginate<T> eşitlik semantiği)

---

## Context

`Paginate<T>` ve `Result<T>` için `readonly record struct` geçişi planlanmıştı.

Ön-mortem analizi iki kritik sorun ortaya çıkardı:

1. **Result<T> — default constructor problemi:**  
   C# `struct` türlerinde parameterless constructor engellenemez.  
   `new Result<T>()` → `{ IsSuccess=false, Value=null, Error=null }` geçersiz durum üretir.  
   Mevcut `sealed class` tasarımı private constructor ile bu garantiyi sağlıyor.

2. **Paginate<T> struct → boxing:**  
   `IReadRepository.GetListAsync` dönüş tipi `IPaginate<T>`.  
   Struct'ı interface'e assign etmek boxing üretir → struct'ın performance faydası sıfırlanır.

---

## Decision

### Paginate<T> → `sealed record class`

`sealed record` seçimi:
- `with` operatörü (immutable transformations)
- Yapısal eşitlik (test assertions kolaylaşır)
- Interface implementasyonu boxing yaratmaz (referans tipi)
- Null güvenliği bozulmaz
- `init` property semantiği korunur

```csharp
// v2.x
public class Paginate<T> : IPaginate<T> { ... }

// v3.0
public sealed record Paginate<T> : IPaginate<T> { ... }
```

**Breaking:** Referans eşitliği `==` kullanan kod, artık yapısal eşitlik alır.  
Pratikte Paginate eşitlik karşılaştırması nadirdir; risk düşük.

### Result<T> → Değişmez

`Result<T>` mevcut `sealed class` tasarımıyla kalır.

Gerekçe:
- Default constructor sorunu çözümsüz (C# dil kısıtı)
- Mevcut tasarım zaten immutable, private ctor ile "always valid" garantisi var
- Boxing endişesi yok (zaten referans tipi)
- Breaking change'in faydası marjinal

---

## Consequences

**Pozitif:**
- `Paginate<T>` artık `with` destekliyor: `var next = page with { Index = page.Index + 1 }`
- Test assertions'da yapısal eşitlik kullanılabilir

**Negatif:**
- `Paginate<T>` üzerinde `==` davranışı değişti (referans → yapısal)
- `Paginate<T>`'yi `is` veya direct cast ile kullanan kod etkilenmez

---

## İlgili ADR'lar

- ADR-0018: v1 breaking changes
- ADR-0019: drop net8.0 target
