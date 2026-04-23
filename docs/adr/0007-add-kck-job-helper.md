# ADR-0007: `AddKckJob<TJob>()` Helper for Background Job Registration

Tarih: 2026-04-20
Durum: Onaylandi

## Baglam

`IBackgroundJob` turunde kullanici tanimli jobs'lar, hem Hangfire hem de Quartz provider'lari tarafindan aynen calistirilabilmeli. Ancak jobs'larin DI lifetime'i kritik: her tetikleme kendi `DbContext` + scoped bagimliliklari almali. Yanlis lifetime (singleton veya transient) race condition veya `DbContext` lifetime ihlallerine neden olur.

Mevcut durumda her kullanici kendi projesinde `services.AddScoped<MyJob>()` yazmak zorundaydi — kolayca atlanir veya yanlis lifetime secilir.

## Karar

`Kck.BackgroundJobs.Abstractions` paketinde provider-agnostic bir helper tanimla:

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class BackgroundJobRegistrationExtensions
{
    public static IServiceCollection AddKckJob<TJob>(this IServiceCollection services)
        where TJob : class, IBackgroundJob
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<TJob>();
        return services;
    }
}
```

- `TryAddScoped` kullanilir — override-friendly, idempotent.
- `Microsoft.Extensions.DependencyInjection` namespace'i — discoverability icin (using gerektirmez).
- Helper hem Hangfire hem Quartz provider'larinda ayni semantikle calisir.

## Alternatiflier Degerlendirildi

1. **Provider spesifik helper (AddHangfireJob, AddQuartzJob):** Reddedildi — kullanici code'u provider'a bagli kalir, provider degistirmek imkansiz.
2. **Otomatik assembly tarama:** Reddedildi — acik registration tercih edildi; otomatik tarama surprise davranis ve build-time garantisizligi getiriyor.
3. **AddScoped (TryAddScoped degil):** Reddedildi — test senaryolarinda `services.AddKckJob<TJob>()` iki kez cagrilabilir, duplicate registration hataya sebep olur.

## Sonuclar

Olumlu:
- Kullanici tek satirla jobs'u register eder, lifetime yanliligi imkansiz.
- Hangfire ↔ Quartz gecisi kullanici code'unda sifir degisiklik gerektirir.
- Idempotent — double registration zararsiz.

Olumsuz:
- `Microsoft.Extensions.DependencyInjection.Abstractions` PackageReference abstraction katmanina eklendi (gerekli, zararsiz).
- Global namespace kullanmak discoverability saglar ama Kck disinda bir `AddKckJob` genisletme yazilmasi riski teorik olarak var (dusuk olasilik, naming prefix'i koruyor).
