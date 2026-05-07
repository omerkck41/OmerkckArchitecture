# ADR-0020: Kck.Caching.Hybrid Provider (HybridCache)

Tarih: 2026-05-07
Durum: Onaylandi

## Bağlam

`Kck.Caching.Redis` yalnızca L2 (Redis) katmanını yönetir. Her `GetAsync`/`SetAsync`
çağrısı ağ üzerinden gider; sık erişilen veriler için gereksiz network round-trip ve
serialization yükü oluşur.

.NET 9 ile kararlı hale gelen `Microsoft.Extensions.Caching.Hybrid` (HybridCache);
L1 (in-process bellek) + L2 (Redis) iki katmanlı önbellekleme, stampede koruması ve
`IBufferDistributedCache` ile sıfır-kopya veri transferi sunar.

## Karar

`Kck.Caching.Hybrid` adında yeni bir provider paketi eklendi. `ICacheService`
implementasyonu olan `HybridCacheService`:

- **GetAsync** → L2 (IDistributedCache): geriye dönük uyumluluk için.
- **SetAsync** → `HybridCache.SetAsync`: L1 + L2'yi birlikte doldurur.
- **RemoveAsync** → `HybridCache.RemoveAsync`: L1 + L2'den birlikte siler.
- **ExistsAsync** → Redis `KeyExistsAsync` (O(1), payload transfersiz).
- **RemoveByPrefixAsync** → Redis SCAN + `HybridCache.RemoveAsync` (L1 eviction).
- **GetOrSetAsync** → `CacheServiceBase` stripe-lock implementasyonu; `SetAsync`'in
  HybridCache'e yönlendirilmesi sayesinde sonuç L1'e yazılır.

## Gerekçe

1. **L1 cache:** Hot-path veriler bir sonraki okumada ağa gitmeden bellekten döner.
2. **Geri uyumluluk:** `ICacheService` arayüzü değişmez; mevcut consumer'lar sıfır
   değişiklikle `AddKckCachingHybrid` çağrısıyla geçiş yapabilir.
3. **Doğru eviction:** `RemoveAsync` hem L1 hem L2'yi geçersiz kılar.

## Sınırlamalar

- `GetAsync` L1'i sorgulamaz (HybridCache, doğrudan Get API'sini açık etmez).
  Maksimum L1 faydası için `GetOrSetAsync` (factory pattern) tercih edilmelidir.
- `RemoveByPrefixAsync` Redis SCAN gerektirir; Redis olmadan kullanılamaz.

## Sonuçlar

**Olumlu:**
- Sık erişilen veriler için network round-trip azalır.
- `SetAsync` L1'i doldurur → sonraki `GetOrSetAsync` çağrıları daha hızlı.
- Stampede koruması `CacheServiceBase` stripe-lock + HybridCache katmanından gelir.

**Olumsuz:**
- `Microsoft.Extensions.Caching.Hybrid 10.1.0` bağımlılığı eklendi.
- `GetAsync` yalnızca L2 sorgular — saf Get-only trafiğinde Redis'ten fark yoktur.

## Referanslar

- [Microsoft HybridCache belgeleri](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- Library Strategy Raporu §4.5
