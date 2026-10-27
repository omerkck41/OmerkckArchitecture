# ADR-0028 — v4.0.0 Kapsamı: Duyurulan Kaldırmalar ve Varsayılan Yolun Lisansı

**Status:** Accepted  
**Date:** 2026-10-27  
**Deciders:** @omerkck41  
**Breaking:** Evet (major sürüm)

---

## Context

3.x boyunca kaldırılmak üzere işaretlenen API'ler birikti ve v3.6.0 sürüm notu bunları 4.0.0 için duyurdu
(`CHANGELOG.md` 3.6.0, `docs/policies/deprecation.md`). Deprecation politikası kaldırmayı yalnız major sürüme bağlar
(ADR-0010). Ayrıca:

1. **Lisans:** `Kck.Pipeline.MediatR`, ticari lisanslı MediatR 13+'ya bağlı (ADR-0021). Lisans kapısındaki istisnası
   2027-01-31'de biter (`decisions/2026-10-26-license-gate.md`). Kural: projede ve varsayılan kullanım yolunda ücretli
   lisans yok (yol haritası D0).
2. **Güvenlik:** `GetClaimsFromToken` (KCK0001) imzayı doğrulamaz; yetkilendirmede kullanılırsa sahte token kabul edilir.
3. **KCK0100 tasarım borcu:** ADR-0016 bool bayraklı `IReadRepository` metotlarını `QueryOptions` ile değiştirdi; ancak
   önerilen `ReadRepositoryExtensions` metotları işi hâlâ eski arayüz üyelerine devrediyor. Eski üyeler bu yüzden düz
   silinemez.
4. **Bundle lisansı ve sessiz varsayılanlar:** `Kck.Bundle.WorkerService` varsayılan olarak Hangfire (LGPL-3.0) kurar;
   `Kck.Documents.ImageSharp` tanınmayan bir format adını sessizce JPEG'e çevirir.
5. **Major bağımlılıklar:** Quartz 3.22 → 4.6 (`Kck.BackgroundJobs.Quartz` 4.x ile derlenmiyor), StackExchange.Redis
   2.13 → 3.4, YamlDotNet 16 → 18, FluentFTP 54 → 55.

---

## Decision

v4.0.0 şunları yapar (uygulama sırası: `docs/plans/2026-10-27-v4-plan.md` §6):

| # | Karar |
|---|---|
| K1 | `Kck.Pipeline.MediatR` paketi kaldırılır; lisans kapısındaki MediatR istisnası silinir. Yerine `Kck.Pipeline.Mediator` (MIT). |
| K2 | KCK0001: `ITokenService.GetClaimsFromToken` ve `JwtTokenService.GetClaimsFromToken` kaldırılır. Yerine `ValidateTokenAsync`. |
| K3 | KCK0100 (D7a): `QueryOptions` alan 6 metot (`GetAsync`, `GetListAsync`, `GetListByDynamicAsync`, `AnyAsync`, `GetByIdAsync`, `CountAsync`) `IReadRepository<T, TId>` üyesi olur; bool bayraklı eski 6 üye ve `ReadRepositoryExtensions` sınıfı silinir. `repo.GetAsync(predicate, options)` biçimindeki çağrılar değişmeden derlenir. |
| K4 | `Kck.EventBus.InMemory` eski kayıt metotları (`AddKckEventBusInMemory` ×2) kaldırılır. Yerine `AddKckEventBus(b => b.UseInMemory())`. `Kck.Core.Abstractions/Pipeline` altındaki boş dosyalar silinir. |
| K5 | `Kck.Bundle.WorkerService` (D3, D8a): varsayılan zamanlayıcı Quartz; Hangfire referansı ve `HangfireWorkerCount` kaldırılır. `"Provider": "Hangfire"` yapılandırması başlangıçta, ne yapılacağını söyleyen bir `InvalidOperationException` verir. |
| K6 | `Kck.Documents.ImageSharp` (D9a): tanınmayan format adı `NotSupportedException` (desteklenen formatları listeler); `"jpg"` ve `"jpeg"` JPEG; boş/null format JPEG (belgeli varsayılan). |
| K7 | Major bağımlılıklar (D10a, koşullu): Quartz 4, StackExchange.Redis 3, YamlDotNet 18, FluentFTP 55 her biri kendi currency-check karar dosyası ve uyum testiyle girer. Testi geçmeyen 4.x minor'a ertelenir; v4.0.0 onu beklemez. StackExchange.Redis 3, `Microsoft.Extensions.Caching.StackExchangeRedis` ile Redis entegrasyon testleri yeşil olmadan girmez. |
| K8 | Kasıtlı kırılmalar proje başına `CompatibilitySuppressions.xml` ile belgelenir; 4.0.0 yayınından sonra silinir ve paket doğrulama tabanı 4.0.0 olur. |
| K9 | 3.x bakım dalı `release/3.x` (v3.6.0'dan). Yalnız güvenlik/kritik düzeltmeler; yayın sonrası H8 kuralı 3.x'in en yenisini listede tutar. |
| K10 | Geçiş rehberi `docs/migrations/v3-to-v4.md`: her kaldırma için önce/sonra örneği; 4.0.0 yayını rehber hazır olmadan yapılmaz. |

---

## Değerlendirilen alternatifler

- **KCK0100 için deprecation'ı geri almak** (eski üyelerden `[Obsolete]` kaldırmak): bool bayraklı API'yi kalıcılaştırır,
  ADR-0016'nın amacını boşa çıkarır. Elendi.
- **KCK0100 eski üyelerini `[EditorBrowsable(Never)]` ile gizli tutmak:** ölü yüzey her uygulayıcıya sonsuza dek yük.
  Elendi.
- **WorkerService'te Hangfire yapılandırmasını sessizce Quartz'a çevirmek:** üretimde zamanlanmış işlerin fark
  edilmeden kaybolmasına yol açar. Elendi.
- **Major bağımlılıkları v4'ün dışında tutmak:** Kck API'sine yansıyan değişiklikler (Quartz) sonra yeni bir major
  gerektirir. Koşullu dahil etmek seçildi.
- **v4 için uzun ömürlü ayrı dal:** Dependabot ve düzeltmeler iki dala bölünür; büyük birleştirme riski. Elendi; main
  üzerinde küçük PR dizisi + `release/3.x` seçildi.
- **v4 boyunca paket doğrulamayı kapatmak:** kasıtlı olmayan ek kırılmalar da geçerdi. Elendi.

---

## Consequences

- (+) Varsayılan kullanım yolunda ücretli veya LGPL lisanslı bağımlılık kalmaz; lisans kapısında MediatR istisnası biter.
- (+) İmza doğrulamayan token API'si ortadan kalkar.
- (+) `IReadRepository` yüzeyi küçülür; tek sorgu modeli `QueryOptions`.
- (−) `Kck.Pipeline.MediatR` kullananlar `Kck.Pipeline.Mediator`'a geçmek zorunda (rehber: `docs/migrations/mediatR-to-mediator.md`).
- (−) `IReadRepository`'yi kendisi uygulayan tüketiciler yeni imzaları uygulamalı.
- (−) WorkerService'i Hangfire ile kullananlar `Kck.BackgroundJobs.Hangfire`'ı doğrudan eklemeli.
- (−) Quartz 4'ün davranış değişiklikleri (varsayılan saat dilimi, misfire, iş kimliği) tüketici işlerini etkileyebilir;
  rehberde ayrı bölüm.
- Risk ve önlemler: `docs/plans/2026-10-27-v4-plan.md` §8 (premortem).

---

## İlgili

- ADR-0010 (deprecation politikası), ADR-0016 (QueryOptions API), ADR-0021 (MediatR deprecasyonu), ADR-0027 (yayın onay kapısı)
- `docs/plans/2026-10-26-dis-inceleme-yol-haritasi.md` (D0, D2–D4), `docs/plans/2026-10-27-v4-plan.md` (D7–D10)
