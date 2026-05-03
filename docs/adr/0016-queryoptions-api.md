# ADR-0016: QueryOptions API — IReadRepository Bool Bayraklarını Kaldırma

Tarih: 2026-05-03
Durum: Onaylandi

## Baglam

Library Strategy raporu (2026-04-25) Bölüm 4.2 ve 4.3:

- **4.2 (HIGH):** `IReadRepository<T,TId>` metodlarında `bool withDeleted` ve
  `bool enableTracking` pozisyonel bayraklar var. Her çağrı sitesinde anlamını
  yitiriyor: `GetAsync(pred, null, false, true)` — hangi bool ne anlama geliyor?
- **4.3 (HIGH):** `Result<T>` fonksiyonel pipeline eksik — `Map`, `Bind`, `Tap`,
  `Ensure` extension'ları yok; caller'lar iç içe `if result.IsSuccess` blokları
  yazıyor.

`QueryOptions` record struct LS-FAZ-4'te `EfRepository.Query()` için eklenmişti
(`Tracking`, `WithDeleted` static factory'leriyle). Ancak `IReadRepository`
interface'ine taşınmamıştı.

## Karar

### 1. ReadRepositoryExtensions — QueryOptions Overload'lar (4.2)

Yeni `ReadRepositoryExtensions` static class, `IReadRepository<T,TId>` üzerinde
6 extension method sunar; her biri `QueryOptions options` parametresi alır:

```csharp
// Önce (bool noise):
await repo.GetAsync(predicate, includes: null, withDeleted: false, enableTracking: true);

// Sonra (self-documenting):
await repo.GetAsync(predicate, QueryOptions.Tracking);
await repo.GetAsync(predicate, QueryOptions.WithDeleted);
await repo.GetAsync(predicate, QueryOptions.None);
```

Extension metot imzaları pozisyonel çakışmayı önlemek için `QueryOptions`'ı
`bool` parametrelerinin yerine zorunlu (default'suz) olarak konumlandırdı:
- `GetAsync(pred, QueryOptions, includes?, ct)` — bool overload ile örtüşmez
- `GetListAsync(QueryOptions, pred?, orderBy?, includes?, index, size, ct)`
- `GetListByDynamicAsync(dynamic, QueryOptions, pred?, includes?, index, size, ct)`
- `AnyAsync(QueryOptions, pred?, ct)`
- `GetByIdAsync(id, QueryOptions, ct)`
- `CountAsync(QueryOptions, pred?, ct)`

### 2. IReadRepository [Obsolete] — KCK0100 (4.2)

6 bool-parametreli metot `[Obsolete(DiagnosticId = "KCK0100")]` ile işaretlendi.
DiagnosticId sayesinde `#pragma warning disable KCK0100` ile hassas susturma
mümkün (CS0618 yerine). `EfRepository` implementasyonu değiştirilmedi — C# `[Obsolete]`
interface üye implementasyonlarını uyarmaz, yalnızca call site'ları uyarır.

### 3. Result<T> Fonksiyonel Pipeline (4.3)

`ResultExtensions` static class eklendi:
- `Map<U>(Func<T,U>)` — success path'te T→U dönüşümü
- `Bind<U>(Func<T,Result<U>>)` — chaining, her adım başarısız olabilir
- `Tap(Action<T>)` — side-effect (logging, metrics), değeri geçirir
- `Ensure(Func<T,bool>, Error)` — postcondition guard

`Ensure` metodunda `error` parametresi için null-guard eklendi (FAZ-6 düzeltmesi).

### 4. QueryOptions.None (4.2)

`QueryOptions.None` static property eklendi — `QueryOptions(false, false)` için
explicit isimli factory. `Tracking` ve `WithDeleted`'e paralel okuma kolaylığı.

## Alternatifler

| Alternatif | Neden Reddedildi |
|---|---|
| Default Interface Methods (DIM) ile QueryOptions | DIM concrete type üzerinden çağrılamaz; cast zorunlu; EfRepository'yi güncellemeyi gerektirir |
| IReadRepository'ye doğrudan yeni overload | Breaking change — implementor zorunlu yeni metot implement etmeli; DIM aynı sorun |
| [EditorBrowsable(Never)] bool metodlara | Roslyn analyzer entegrasyonu yok; IDE gizler ama build uyarmaz |

## Sonuçlar

- **Pozitif:** Call site okunabilirliği artar; tip güvenli sorgu konfigürasyonu;
  KCK0100 ile suppress edilebilir; EfRepository değişmedi (non-breaking).
- **Negatif:** `_sut.GetAsync(pred)` gibi tek-argümanlı çağrılar hâlâ eski
  (obsolete) interface metoduna çözümlenir — concrete type üzerinden çağıranlarda
  CS0618/KCK0100 üretmez (sadece interface referansından çağıranlarda uyarı çıkar).
- **Kapsam dışı:** `EfRepository`'nin QueryOptions metotlarını override etmesi
  (performans optimizasyonu) — FAZ-8 veya ayrı PR kapsamı.
