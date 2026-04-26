# ADR-0015: Performans Hizli Kazanimlar (LS-FAZ-5)

Tarih: 2026-04-26
Durum: Onaylandi

## Baglam

Library Strategy raporu (2026-04-25) Bolum 4.4, 5.5, 5.6, 5.4:

- **5.6 (P1):** `RedisCacheService.ExistsAsync` `IDistributedCache.GetStringAsync`
  cagiriyor — sadece varlik kontrolu icin **butun degeri** ag uzerinden cekiyor.
  1 MB cache item icin var/yok kontrolu = 1 MB network + 1 MB GC. Ciddi yik.
- **5.5:** `Filter.GetValue<T>` `Convert.ChangeType` kullaniyor — AOT-incompatible,
  `Guid`/`DateTimeOffset`/`Enum`/`DateOnly`/`TimeOnly` desteklemez, JSON
  deserialize sonrasi gelen `JsonElement` degerleri atilir.
- **4.4:** `Filter.Operator` string — type safety yok, typo'lar runtime'da
  patlar. `FilterOperator` enum'u VAR ama Filter ile entegre degil.
- **5.4:** CA2007 ConfigureAwait analyzer'i `.editorconfig`'de yok; kontrol
  manuel.

## Karar

Uc maddeyi LS-FAZ-5'te uyguladik, biri **bilincli ertelendi**:

### 1. Redis ExistsAsync — KEY EXISTS (5.6)

```csharp
// Once: butun payload network'e cikiyor
return await cache.GetStringAsync(BuildKey(key), ct).ConfigureAwait(false) is not null;

// Sonra: sadece EXISTS komutu
ct.ThrowIfCancellationRequested();
return await redis.GetDatabase().KeyExistsAsync(BuildKey(key)).ConfigureAwait(false);
```

`IConnectionMultiplexer redis` zaten primary constructor'da mevcut.
`KeyExistsAsync` CT almıyor (StackExchange.Redis API limiti); pre-check ile
erken-cancel saglandi. Mevcut `RemoveByPrefixAsync.KeyDeleteAsync` ile uyumlu pattern.

### 2. Filter Type Safety (4.4 + 5.5)

`Filter.cs` rewrite — additive degisiklikler:

- **`GetValue<T>`** switch + `ParseString<T>` ile AOT-uyumlu:
  - `JsonElement` -> native `Deserialize<T>`
  - `string` -> tip-spesifik parser (Guid, DateTime, DateTimeOffset, DateOnly,
    TimeOnly, decimal, Enum, vd.)
  - Bilinmeyen tipler -> `Convert.ChangeType` fallback (geri-uyumlu)
- **Yeni constructor:** `Filter(string field, FilterOperator op, ...)` —
  enum'u canonical lower-case wire string'e cevirir
- **Yeni property:** `OperatorEnum` -> `FilterOperator?` (null = tanimsiz)
- **Yeni static helper:** `TryParseOperator(string?)` -> `FilterOperator?`

**Wire format degismedi** — JSON deserialize bozulmaz, mevcut consumer'lar
etkilenmez.

`DynamicFilterExtensions.cs:52` `Enum.Parse<FilterOperator>(...)` artik
`filter.OperatorEnum ?? throw` kullaniyor — convenience kullanim ornegi.

PublicAPI tracking (LS-FAZ-3 ADR-0013): 3 yeni sembol `PublicAPI.Unshipped.txt`'a
eklendi.

### 3. CA2007 ConfigureAwait Enforcement — ERTELENDI (5.4)

Plan: `.editorconfig`'e `[src/**/*.cs]` scope'unda `dotnet_diagnostic.CA2007.severity = warning`.

**Beklenti:** rapor "tum await'lerde mevcut" diyordu, sweep ~0 site verecekti.

**Gercek:** 138 site, 9 projede:

| Proje | Site |
|---|---|
| Kck.Pipeline.MediatR | 56 |
| Kck.Persistence.EntityFramework | 48 |
| Kck.Security.Argon2 | 8 |
| Kck.Observability.OpenTelemetry | 8 |
| Kck.Security.{Jwt} + Kck.Exceptions.AspNetCore + Kck.BackgroundJobs.{Quartz,Hangfire} | 4 her biri |
| Kck.AspNetCore | 2 |

Plan icindeki contingency uygulanmadi (>5 site → rollback) ve **ayri PR**'a
(LS-FAZ-5.5) ayrildi.

**Onemli:** ASP.NET Core middleware'lerde (`Kck.AspNetCore`,
`Kck.Exceptions.AspNetCore`) ConfigureAwait(false) gerekmez (
SynchronizationContext yok). LS-FAZ-5.5 scope karari her proje icin ayri
verilmeli.

## Alternatifler Degerlendirildi

### Filter API ekleme yontemi (4.4)

1. **`string Operator` -> `FilterOperator Operator`** (breaking): Reddedildi —
   wire format degisirdi, mevcut JSON consumer'lar bozulurdu.
2. **`Filter` immutable record yap (`required init`):** Reddedildi — mevcut
   POCO-style `set` kullanan kod parcalari var.
3. **Builder pattern:** Reddedildi — Filter zaten kucuk POCO, builder ise
   karmasaya cevirirdi. Mevcut yapilara additive ekleme daha az risk.

### GetValue<T> tip kapsami (5.5)

1. **`Convert.ChangeType` korumak:** Reddedildi — AOT motivasyonu var.
2. **Tum tipleri source-gen ile uretmek:** Reddedildi — over-engineering;
   tipik tipler (Guid, DateTime, vb.) yeterli.
3. **Source-gen JsonContext:** LS-FAZ-6 hedefi (Bolum 5.1) — burada baseline.

### CA2007 sweep zamani

1. **Tek PR icinde sweep + enforcement:** Reddedildi — 138 site degisikligi
   review icin cok buyuk; LS-FAZ-5 odagi (perf quickwin) ile karistiramaz.
2. **Sadece enforcement, mevcut sitelar suppressed:** Reddedildi — yeni await
   yazilirken onlemez ama eski siteleri unutturmadi; geri donus borcu birakir.
3. **Ayri PR (LS-FAZ-5.5):** **Secildi** — sweep ve enforcement birlikte
   ayri review.

## Sonuclar

**Olumlu:**
- Buyuk-degerli Redis cache'lerde `ExistsAsync` cagrilarinda %95+ network
  azalma (RTT bazli).
- Filter JSON-tabanli kullanimda artik `JsonElement`'i native deserialize
  ediyor — eskiden cast hatasi atip atmamasi tesaduftu.
- AOT-uyumluluk ozelikle PublishAot deneyenler icin acildi (en azindan
  yaygin tipler).
- `FilterOperator` enum'u public API'ya entegre — type safety + IDE
  IntelliSense.

**Olumsuz:**
- Filter `RS0026` supresyonu kaldi — gelecekte iki ctor parametre listesini
  birlikte tutmak gerekir.
- CA2007 sweep ertelendi → 138 site hala manuel-bagimli.
- `OperatorEnum` getter her cagrida `Enum.TryParse` calistiriyor (cache yok);
  hot-path'te degil — kabul edilebilir.

## Referanslar

- `tasks/library-strategy-2026-04-25.md` Bolum 4.4, 5.4, 5.5, 5.6
- `tasks/library-strategy-faz5-performance-quickwins-2026-04-26.md`
- ADR-0013 (Public API Discipline)
- [StackExchange.Redis IDatabase.KeyExistsAsync](https://github.com/StackExchange/StackExchange.Redis)
- [Microsoft CA2007](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2007)
- [Roslyn PublicApiAnalyzers RS0026](https://github.com/dotnet/roslyn/blob/main/docs/Adding%20Optional%20Parameters%20in%20Public%20API.md)
