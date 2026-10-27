# Karar: StackExchange.Redis 2.13 → 3.4 (Kck 4.0)

- Tarih: 2026-10-27 · Bağlam: ADR-0028 K7, v4 planı PR 3 (H11-b), kabul örneği V8
- Ön beyan: aday 3.4.20; ölçütler — izin verici lisans, net10.0, arşivlenmemiş repo, açık güvenlik bildirimi yok,
  `Microsoft.Extensions.Caching.StackExchangeRedis` (MECSER) ile ikili uyum **gerçek Redis'te** kanıtlanır (V8). Geçmezse
  2.x'te kalınır (Plan B). Alternatif: 2.13.x'te kalmak.
- Registry: `StackExchange.Redis` deprecated-registry'de yok.

| Aday | Sürüm | Lisans | Sinyal | Karar |
|---|---|---|---|---|
| StackExchange.Redis | 3.4.20 (2026-10-23) | MIT (NuGet); depo `NOASSERTION` = MIT + üçüncü taraf bildirimleri (LICENSE okundu) | repo aktif (2026-10-27), güvenlik bildirimi 0 | **Seçildi (V8 koşuluyla)** |
| RESPite (yeni geçişli bağımlılık) | 3.4.20 | MIT | aynı yayıncı (Stack Exchange) | kabul — lisans kapısı PASS |
| 2.13.17'de kalmak | 2.13.17 | MIT | 2.x bakım hattı | Plan B |

## Uyum ve davranış
- 3.0.0 sürüm notu: "Intentionally, there are no API changes in this release; it is a direct mirror of 2.13.17, but with
  the new IO core." Tek istisna: `Execute[Async]` artık `AllowAdmin` gerektiren komutları istemcide reddeder.
- MECSER 10.0.x bağımlılık aralığı `StackExchange.Redis [2.7.27, )` — 3.x'e izin verir.
- Paket doğrulama (taban 3.6.0): Kck public API'sinde değişiklik yok, suppression gerekmez.
- Tüketici davranışı (geçiş rehberine): **RESP3 varsayılan** (RESP2'ye zorlamak için `ConfigurationOptions.Protocol`);
  RESP3'te pub/sub ayrı bağlantı kullanır (3.x); Redis Enterprise/AMR'de smart client handoff varsayılan açık;
  `Execute("FLUSHALL")` gibi yönetici komutları `AllowAdmin` olmadan reddedilir.

## Doğrulama
- Yerel (2026-10-27): kod değişikliği gerekmedi; `restore --force-evaluate` + `--locked-mode`, Release build 0 hata, birim
  testleri 0 başarısız (Redis 7/7, Hybrid 8/8, TokenBlacklist 8/8), pack + paket doğrulama hatasız, lisans kapısı PASS.
- **V8 (gerçek Redis, CI ubuntu, Testcontainers `redis:7-alpine`):** `RedisCacheIntegrationTests` (MECSER + SE.Redis),
  `RedisTokenBlacklistIntegrationTests` ve yeni `HybridCacheIntegrationTests` (HybridCache → MECSER → SE.Redis 3; başka
  örnekten L2 okuma, `RemoveByPrefixAsync`). Yerelde Docker yok → sonuç PR CI'ında; kırmızıysa Plan B.
- Yan bulgu: V8 hazırlığında `Kck.Caching.Hybrid.GetAsync` hatası bulundu ve ayrı düzeltildi (#186, 3.6.2 #187).

## Kaynaklar (erişim 2026-10-27)
- https://github.com/StackExchange/StackExchange.Redis/releases (3.0.0 … 3.4.20; 3.0.0 notu)
- http://seredis.dev/ReleaseNotes ("From 3.0, release notes will be maintained in GitHub only")
- https://api.nuget.org/v3/registration5-semver1/stackexchange.redis/3.4.20.json, `.../respite/3.4.20.json`
- `gh api repos/StackExchange/StackExchange.Redis` (archived=false), `/contents/LICENSE`, `/security-advisories` (0)
- EOL: endoflife.date kaydı yok → N/A; .NET 10 LTS 2028-11-14
