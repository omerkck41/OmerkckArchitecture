# ADR-0004: IOptions<T> → IOptionsMonitor<T> Migration

Tarih: 2026-04-20
Durum: Onaylandi

## Baglam

Tum provider'lar yapilandirma icin `IOptions<TOptions>` enjekte ediyor ve
constructor'da `options.Value` okuyordu. `IOptions<T>` singleton olarak
cozumlenir ve degeri bir kez snapshot'lar — `appsettings.json` veya baska
bir konfigurasyon kaynagi runtime'da degisse bile uygulama eski degeri
kullanmaya devam eder.

Audit raporu (2026-04-20, HIGH-3) bu anti-pattern'i 18 provider'da isaretledi:
tum ayar degisiklikleri ancak uygulama yeniden baslatildiginda etkili
olabiliyordu. Hot-reload destegi, Key Vault / secret rotation senaryolari ve
feature flag benzeri dinamik konfigurasyonlar engelleniyordu.

## Karar

Tum provider'lar `IOptionsMonitor<TOptions>` kullanacak sekilde refactor edildi.
`options.Value` cagrilari `options.CurrentValue` olarak degistirildi.

Kapsam:
- 18 src dosyasi (Jwt, Redis, Hangfire, Quartz, RabbitMQ, Argon2, Totp, ...)
- 2 DI factory kaydi (Localization Json/Yaml `sp.GetRequiredService<IOptions<T>>()` →
  `IOptionsMonitor<T>`)
- 8 test dosyasi `Options.Create(...)` → paylasilan `StaticOptionsMonitor<T>` helper'i
- Test paylasim stratejisi: `tests/_Shared/StaticOptionsMonitor.cs` + `tests/Directory.Build.props`
  icinde `<Compile Include ... Link>` — helper her test projesine otomatik akar,
  duplikasyon yok.

`CachingBehaviorTests.cs:29` harici Microsoft tipi
(`MemoryDistributedCache(IOptions<MemoryDistributedCacheOptions>)`) kullandigi
icin degistirilmedi.

## Alternatifler Degerlendirildi

- **IOptions<T>'ta kal:** Hot reload imkani kaybediliyor, secret rotation
  senaryosu calismiyor. Reddedildi.
- **IOptionsSnapshot<T>:** Yalniz scoped servislerde kullanilabilir;
  singleton provider'lar (`IHttpClientFactory`'nin arkasindaki handler'lar,
  `IConnectionMultiplexer` vb.) enjekte edemez. Reddedildi.
- **IConfiguration dogrudan:** Tip guvenligi kaybi, validation atlanir.
  Reddedildi.

## Sonuclar

**Olumlu:**
- Tum provider'lar konfigurasyon degisikliklerini restart olmadan yansitir
- Key Vault / secret manager rotation desteklenir
- `OnChange` callback'i ileride gerekirse hazir (ornek: Redis connection
  string degisiminde bagli yeniden baglanma)

**Olumsuz:**
- Public constructor imzasi degisti — breaking change, major version bump
  gerektirir (`0.x.0` icin kabul edilebilir)
- Her `CurrentValue` cagrisi guncel degeri okur — provider icindeki
  hotpath'larda field cache'leme paterni (`_options = options.CurrentValue`
  constructor'da) kullanildi

## Dogrulama

- `dotnet build`: 0 hata, 51 warning (hepsi pre-existing CA)
- `dotnet test`: tum test projeleri yesil
