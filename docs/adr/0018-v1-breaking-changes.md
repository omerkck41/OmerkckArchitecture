# ADR-0018: v1.0 Breaking Changes — Paginate Immutability, ValueTask, Resilience, AOT

Tarih: 2026-05-03
Durum: Onaylandi

## Baglam

v1.0 major bump öncesi son faz. Library Strategy raporu §4.1, §5.3, §3.1, §2.2:

- **§4.1 (HIGH):** `Paginate<T>` tüm property'leri `{ get; set; }` — value-object
  semantiği yok; tüketici kod state'i değiştirebilir.
- **§5.3 (HIGH):** `ICacheService.GetAsync/ExistsAsync` `Task<T>` dönüyor —
  InMemory provider zaten senkron; her çağrıda gereksiz `Task` nesnesi alloc edilir.
- **§3.1 (MEDIUM):** HTTP dışı resilience katmanı yok — yalnızca `Kck.Http.Resilience`
  mevcut; DB, cache, eventbus için retry/circuit-breaker kullanılmıyor.
- **§2.2 (MEDIUM):** Abstraction kütüphaneleri `IsAotCompatible` işaretlemiyor —
  Native AOT derlemelerinde trimming uyarıları gizleniyor.

## Karar

### 1. Paginate<T> Immutability (§4.1) — BREAKING

Tüm 11 property `{ get; set; }` → `{ get; init; }`. `Items` tipi zaten `IReadOnlyList<T>`.
`Create()` factory (object initializer kullanan) ve parametre-alıcı constructor değişmedi.

**Etki:** `paginate.Index = 5` gibi post-construction setter çağrıları derleme hatası verir.
**Migration:** Paginate oluşturmak için `Paginate<T>.Create(...)` factory'sini kullanın.

### 2. ICacheService ValueTask (§5.3) — BREAKING

```csharp
// Önce:
Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
Task<bool> ExistsAsync(string key, CancellationToken ct = default);

// Sonra:
ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default);
ValueTask<bool> ExistsAsync(string key, CancellationToken ct = default);
```

**Etki:** `ICacheService` implementasyonları override imzalarını güncellemeli.
**Çağrı sitesi:** `await cache.GetAsync<T>(...)` sözdizimi değişmez — uyumluluk yüksek.
**Migration:** Override imzalarında `Task<T?>` → `ValueTask<T?>` yapın.

### 3. Kck.Resilience.Polly — Yeni Provider (§3.1)

`Microsoft.Extensions.Resilience` (Polly v8) sarmalayıcı. Adlandırılmış pipeline:

```csharp
services.AddKckResilience("external-api", pipeline => {
    pipeline.AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 });
    pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions());
    pipeline.AddTimeout(TimeSpan.FromSeconds(10));
});

// Kullanım:
var provider = sp.GetRequiredService<ResiliencePipelineProvider<string>>();
var pipeline = provider.GetPipeline("external-api");
await pipeline.ExecuteAsync(ct => DoWorkAsync(ct));
```

Kck.Http.Resilience ile çakışmaz — HTTP dışı (DB, cache, event bus) senaryolara yönelik.

### 4. IsAotCompatible = true — Abstractions (§2.2)

`src/Abstractions/Directory.Build.props`'a `<IsAotCompatible>true</IsAotCompatible>` eklendi.
Tüm 16 abstraction projesi bu flag'i devralır. Build sonucu: 0 yeni trim/AOT uyarısı.
Providers ileri aşamalarda (reflection kullananlar) `[UnconditionalSuppressMessage]` ekler.

## Alternatifler

| Alternatif | Neden Reddedildi |
|---|---|
| `record struct` olarak yeniden yaz | Primary constructor Paginate<T> için çok fazla refactor; value semantic fazla kısıtlayıcı (kalıtım gerektirebilir) |
| `IValueTaskSource<T>` kendin implement et | `ValueTask.FromResult` yeterli; aşırı mühendislik |
| Polly doğrudan kullan (sarmalamadan) | DI kayıt ve keşfedilebilirlik; tekdüze isim uzayı |
| Tüm provider'lara AOT işaret koy | Provider'lar reflection kullanan 3. taraf kütüphanelere bağımlı — kırılgan |

## Migration Rehberi (v0.x → v1.0)

### Paginate<T>

```csharp
// ÖNCE — derleme hatası verir:
var p = new Paginate<MyItem>();
p.Index = 0;
p.Items = myList;

// SONRA — doğru kullanım:
var p = Paginate<MyItem>.Create(myList, totalCount, index: 0, size: 10);
```

### ICacheService Implementasyonu

```csharp
// ÖNCE:
public override async Task<MyEntity?> GetAsync<MyEntity>(string key, CancellationToken ct)
    => ...;

// SONRA:
public override async ValueTask<MyEntity?> GetAsync<MyEntity>(string key, CancellationToken ct)
    => ...;
```

## Sonuçlar

- **Pozitif:** Paginate artık yanlışlıkla mutate edilemiyor; InMemory cache hot-path'te
  zero alloc; generic resilience pipeline kullanılabilir; AOT uyumluluk bildirimi yapıldı.
- **Negatif:** `ICacheService` implementasyonları güncellenmeli — tek satır değişiklik.
- **Teknik Borç:** MediatR → Mediator migrasyonu, Pipeline marker relocation,
  Cache buffer overload'ları, .NET Aspire entegrasyonu sonraki sürüme ertelendi.
