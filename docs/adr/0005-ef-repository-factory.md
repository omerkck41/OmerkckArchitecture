# ADR-0005: EfUnitOfWork Service Locator → IEfRepositoryFactory

Tarih: 2026-04-20
Durum: Onaylandi

## Baglam

`EfUnitOfWork` constructor'inda `IServiceProvider` enjekte ediliyor,
`Repository<T, TId>()` icinde `serviceProvider.GetService<IRepository<T, TId>>()`
cagriliyordu. Bu klasik Service Locator anti-pattern:

- UoW'nun bagimliliklari constructor'dan okunamiyor — tip sisteminde gizli
- Test etmek icin tum container kurulmali, salt `IRepository<,>` mock'u yetmiyor
- UoW teknik olarak tum DI grafigine erisebildigi icin SRP ihlali

Audit raporu (2026-04-20, HIGH-4) bu sinifi isaretledi.

## Karar

`IEfRepositoryFactory` adinda dar bir soyutlama olusturuldu:

```csharp
public interface IEfRepositoryFactory
{
    IRepository<T, TId> Create<T, TId>(DbContext context) where T : Entity<TId>;
}
```

`DefaultEfRepositoryFactory`:
- DI'dan `IRepository<T, TId>` cozmeyi dener (custom override imkani korunur)
- Bulamazsa `IFilterPropertyWhitelist<T>` opsiyonel bagimliligi cozup
  `new EfRepository<T, TId>(context, whitelist)` dondurur

`EfUnitOfWork`:
- Artik `IServiceProvider` yerine `IEfRepositoryFactory` enjekte ediyor
- `Repository<T, TId>()` icinde factory.Create cagriliyor

DI kaydi `KckPersistenceBuilder.UseEntityFramework<TContext>`'te:
- `services.TryAddScoped<IEfRepositoryFactory, DefaultEfRepositoryFactory>()`

## Alternatifler Degerlendirildi

- **Acik generic registration** (`services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>))`):
  Runtime'da T per-generic instance pre-inject edilemez; UoW dinamik T ile
  calistigi icin yine bir tur dispatch lazim. Reddedildi.
- **Typed factory (`Func<DbContext, IRepository<T, TId>>`)**: C# generic
  delegate'ler constructor injection'da rahat cozulmuyor. Reddedildi.
- **IServiceScopeFactory**: Her Repository cagrisinda yeni scope acmak
  EF Core change tracking'i bozar. Reddedildi.

## Sonuclar

**Olumlu:**
- UoW'nun bagimliligi typed: constructor imzasi gercegi soyluyor
- Service Locator kullanimi tek yere (factory) izole — orasi pattern'in
  uygun evi
- Custom repository overriding hala mumkun (DI'ya
  `IRepository<CustomEntity, Guid>` kaydet)
- `IFilterPropertyWhitelist<T>` cozumlemesi merkezi bir yerde

**Olumsuz:**
- `EfUnitOfWork` public constructor imzasi degisti — breaking change
- Fabrika kaydi `UseEntityFramework` tarafindan yapiliyor, `TryAdd`
  kullanildi; ozel factory gerekenler `KckPersistenceBuilder` oncesi kayit
  yapabilir

## Dogrulama

- `dotnet build`: 0 hata
- `Kck.Persistence.EntityFramework.Tests` (13 test) yesil
- `new EfUnitOfWork(...)` dogrudan cagrisi test'lerde yok — yalniz
  DI uzerinden kullaniliyor; breaking change downstream'e dusmuyor
