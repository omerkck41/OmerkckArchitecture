# ADR-0009: `TryAddSingleton` Consistency for Single-Implementation Services

Tarih: 2026-04-20
Durum: Onaylandi

## Baglam

Provider `ServiceCollectionExtensions` dosyalarinda service kayitlari tutarsizdi: bazi yerlerde `AddSingleton`, bazi yerlerde `TryAddSingleton` kullaniliyordu. `AddSingleton` duplicate kayit olusur — kullanici override etmek isterse (ornegin test double), hem gercek hem test implementasyonu container'a girer ve `GetRequiredService<T>()` sonuncu kazanan semantigi nedeniyle surprising davranis gosterebilir. Ayrica ayni helper iki kez cagrildiginda duplicate registration olur.

## Karar

Provider paketlerinde **tekil implementasyonu olan tum service kayitlari** `TryAddSingleton` kullanacak sekilde degistirildi. Bu kural:

- `services.AddSingleton<IService, Impl>()` -> `services.TryAddSingleton<IService, Impl>()`
- Her dosyaya `using Microsoft.Extensions.DependencyInjection.Extensions;` eklendi.

Etkilenen providerler (19 dosya):
- Security: Jwt, Argon2, Totp, TokenBlacklist.Redis, Secrets.UserSecrets, Secrets.AzureKeyVault
- Messaging: SendGrid, AmazonSes, MailKit
- Caching: InMemory, Redis
- EventBus: InMemory, RabbitMq, AzureServiceBus
- BackgroundJobs: Hangfire, Quartz
- Documents: ClosedXml, ImageSharp
- Search: Elasticsearch
- FeatureFlags: InMemory
- Observability: OpenTelemetry (ITracingService, IMetricsService)
- AspNetCore: KckAspNetCoreBuilder (IInputSanitizer, ICookieManager, ISessionManager), builder singleton
- Persistence.EntityFramework: AuditInterceptor
- Exceptions.AspNetCore: GlobalExceptionHandler, ValidationExceptionHandler, ExceptionHandlerFactory

**Istisnai durum (AddSingleton korundu):**
- `Kck.Localization` ailesi (`InMemory`, `Json`, `Yaml`): `IResourceProvider` **kasten multi-impl stacking** kullanir. Framework `IEnumerable<IResourceProvider>` ile hepsini okur. `TryAddSingleton` bu semantigi bozardi.

## Alternatiflier Degerlendirildi

1. **Durumu oldugu gibi birakma:** Reddedildi — tutarsizlik teknik borc, zamanla daha da buyur.
2. **Tum `AddSingleton`'lari `TryAddSingleton` yapma (Localization dahil):** Reddedildi — Localization intentional multi-impl, kirilma riski cok yuksek.
3. **Analyzer/source generator ile zorunlu kilma:** Reddedildi — pratik fayda/maliyet orani dusuk; convention + code review yeterli.

## Sonuclar

Olumlu:
- Kullanici override etme istedigi her servis, kendi registration'ini `ServiceCollection`'a koyup sonra `AddKckProvider()` cagirarak ezebilir (TryAdd idempotent oldugundan).
- Tekrar cagrilmasi halinde duplicate kayit yok.
- Tum provider'larda tutarli registration semantigi.

Olumsuz:
- `TryAddSingleton` imzasi `Microsoft.Extensions.DependencyInjection.Extensions` namespace'i icinde — her dosyaya using eklemek gerekti.
- Localization istisna durumu proje dokumanlarinda acik olarak gosterilmeli ki sonraki refactoring'de bozulmasin.
