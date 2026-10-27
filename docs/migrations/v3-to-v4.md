# Migration v3.x → v4.0

**Etki:** Breaking · **Tahmini effort:** S–M (cogu degisiklik derleyici hatasi olarak gorunur)
**Karar:** [ADR-0028](../adr/0028-v4-scope.md)

## Neden bu major?

3.x boyunca `[Obsolete]` olarak duyurulan API'ler kaldirildi, varsayilan kullanim yolu tamamen izin verici lisansli hale
getirildi (MediatR ve bundle'daki Hangfire cikti) ve major bagimliliklar guncellendi. 3.x'in son surumu nuget.org'da
listede kalir; 3.x yalniz guvenlik/kritik duzeltme alir (`release/3.x`).

## Hizli bakis

| Paket | Degisiklik | Ne yapmali |
|---|---|---|
| `Kck.Pipeline.MediatR` | **Kaldirildi** (MediatR 13+ ticari) | `Kck.Pipeline.Mediator`'a gecin — [rehber](mediatR-to-mediator.md) |
| `Kck.Persistence.*` | `IReadRepository` bool bayrakli metotlari kaldirildi (KCK0100) | `QueryOptions` (asagida tablo) |
| `Kck.Security.*` | `GetClaimsFromToken` kaldirildi (KCK0001) | `ValidateTokenAsync` |
| `Kck.EventBus.InMemory` | `AddKckEventBusInMemory` kaldirildi | `AddKckEventBus(b => b.UseInMemory())` |
| `Kck.Bundle.WorkerService` | Hangfire cikti, varsayilan Quartz | Hangfire icin paketi kendiniz ekleyin |
| `Kck.Documents.ImageSharp` | Bilinmeyen format hata verir; bos format kaynak formatini korur | Format adlarini kontrol edin |
| `Kck.BackgroundJobs.Quartz` | Quartz.NET 4.6 | `Action<IQuartzBuilder>` |
| `Kck.Caching.Hybrid` | `HybridCacheService` ctor'undan `IDistributedCache` cikti | DI kullaniyorsaniz degisiklik yok |
| `Kck.Caching.Redis`, `.Hybrid`, `TokenBlacklist.Redis` | StackExchange.Redis 3 (RESP3 varsayilan) | Asagidaki notlar |

## 1. MediatR

`Kck.Pipeline.MediatR` 4.0'da yayinlanmiyor. 3.x paketi nuget.org'da "deprecated". Gecis:
[mediatR-to-mediator.md](mediatR-to-mediator.md).

## 2. Repository okuma metotlari (KCK0100)

3.x'te `QueryOptions` alan uzanti metotlari artik `IReadRepository<T, TId>` uyesi; bool bayrakli overload'lar ve
`ReadRepositoryExtensions` kaldirildi. `options` verilmezse 3.x varsayilanlari korunur (Get/GetList/GetListByDynamic
tracking yapar; Any/GetById/Count yapmaz).

| 3.x | 4.0 |
|---|---|
| `repo.GetAsync(p, QueryOptions.Tracking)` (uzanti) | ayni — degisiklik yok |
| `repo.GetAsync(p)` / `repo.GetListAsync(size: 10)` | ayni (tracking acik) |
| `repo.GetAsync(p, withDeleted: true)` | `repo.GetAsync(p, new QueryOptions(IncludeDeleted: true, AsTracking: true))` |
| `repo.GetAsync(p, enableTracking: false)` | `repo.GetAsync(p, QueryOptions.None)` |
| `repo.GetByIdAsync(id, withDeleted: true)` | `repo.GetByIdAsync(id, QueryOptions.WithDeleted)` |
| `repo.AnyAsync(p)` / `repo.CountAsync(p)` / `repo.GetListAsync(p)` | `AnyAsync(predicate: p)` vb. — `options` ilk parametre |
| `ReadRepositoryExtensions.GetAsync(repo, ...)` | `repo.GetAsync(...)` |

> **Dikkat:** `QueryOptions.WithDeleted` tracking yapmaz. 3.x'te `GetAsync(p, withDeleted: true)` tracking yapiyordu;
> yukle-degistir-`SaveChanges` akisinda `AsTracking: true` vermezseniz degisiklik sessizce kaydedilmez.

Kendi `IReadRepository` uygulamaniz varsa yeni imzalari uygulayin.

## 3. JWT: `GetClaimsFromToken` (KCK0001)

Imza dogrulamadan claim okuyordu. Yetkilendirme icin `ValidateTokenAsync` kullanin:

```csharp
// Once (3.x)
var claims = tokenService.GetClaimsFromToken(token);

// Sonra (4.0)
var result = await tokenService.ValidateTokenAsync(token, ct);
if (result.IsValid) { /* result.Claims */ }
```

Yalniz log/debug icin imzasiz okuma gerekiyorsa `Microsoft.IdentityModel.JsonWebTokens.JsonWebToken`'i kendiniz
kullanin; bu degeri yetki karari icin kullanmayin.

## 4. InMemory event bus

```csharp
// Once (3.x)
services.AddKckEventBusInMemory(typeof(Program).Assembly);

// Sonra (4.0)
services.AddKckEventBus(b => b
    .UseInMemory()
    .RegisterHandlersFromAssembly(typeof(Program).Assembly));
```

## 5. Worker Service bundle

Bundle yalniz Quartz kullanir ve varsayilani Quartz'dir. 3.x varsayilani Hangfire'di:

- Yapilandirmada `"Kck:BackgroundJobs:Provider"` yoksa uygulama artik **Quartz** ile calisir. 3.x'te Hangfire ile
  zamanlanmis isleriniz varsa bunlar Hangfire depolamasindan tasinmaz.
- `"Provider": "Hangfire"` baslangicta `InvalidOperationException` verir. Hangfire'da kalmak icin:
  `Kck.BackgroundJobs.Hangfire` paketini ekleyin, `AddKckBackgroundJobsHangfire(...)` cagirin ve bundle yerine
  Serilog/OpenTelemetry/event bus kayitlarini kendiniz yapin.
- `BackgroundJobs:HangfireWorkerCount` ayari kaldirildi.

## 6. ImageSharp format kurallari

| Girdi | 3.x | 4.0 |
|---|---|---|
| `avif`, yazim hatasi vb. | sessizce JPEG | `NotSupportedException` |
| bos / `null` format | JPEG | kaynak gorselin formati (kaynak desteklenmiyorsa JPEG) |
| `jpg` | JPEG | JPEG |

## 7. Quartz.NET 4

`AddKckBackgroundJobsQuartz(Action<IServiceCollectionQuartzConfigurator>?)` →
`AddKckBackgroundJobsQuartz(Action<IQuartzBuilder>?)`. Kendi `IJob` siniflariniz varsa imza
`ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)` oldu. Quartz 4 davranis
degisiklikleri: saat dilimi verilmeyen cron tetikleyicileri `DefaultTimeZone` kullanir, misfire varsayilani
`DefaultMisfireHandling`, kimliksiz is/tetikleyici uyari 4030 verir, iptal edilen calisma `Cancelled` kaydedilir.
Ayrinti: [Quartz 4 migration guide](https://www.quartz-scheduler.net/documentation/quartz-4.x/migration-guide.html),
[decisions/2026-10-27-quartz-4.md](../../decisions/2026-10-27-quartz-4.md).

## 8. StackExchange.Redis 3 ve OpenTelemetry 1.20

- **Redis:** RESP3 varsayilan protokol (RESP2 icin `ConfigurationOptions.Protocol`); `AllowAdmin` gerektiren komutlar
  (or. `FLUSHALL`) `AllowAdmin` olmadan istemcide reddedilir.
- **OpenTelemetry:** OTLP exporter'in kendi `HttpClient`'i 3xx yonlendirmelerini izlemez; tek gonderim ust siniri 64 MiB.

## 9. HybridCacheService

`HybridCacheService(HybridCache, IDistributedCache, IConnectionMultiplexer, IOptionsMonitor<HybridCacheOptions>)` →
`HybridCacheService(HybridCache, IConnectionMultiplexer, IOptionsMonitor<HybridCacheOptions>)`.
`AddKckCachingHybrid(...)` ile kaydediyorsaniz degisiklik yok.

## Kontrol listesi

- [ ] `Kck.*` paketlerini 4.0'a yukseltin, derleyin; kalan hatalar yukaridaki bolumlerden birine karsilik gelir.
- [ ] `withDeleted: true` cagrilarinda tracking ihtiyacini kontrol edin (bolum 2).
- [ ] Worker Service: zamanlayici secimini ve kalici is verisini kontrol edin (bolum 5).
- [ ] Redis/OTLP kurulumunuzu bolum 8'e gore dogrulayin.

Sorun: [GitHub Issues](https://github.com/omerkck41/OmerkckArchitecture/issues).
