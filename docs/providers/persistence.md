# Persistence

Entity Framework Core tabanli repository + Unit of Work. Soft-delete filtresi repository sorgularinda uygulanir; audit
ve domain event dispatch icin EF Core interceptor'lari vardir.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Persistence.Abstractions` | `IRepository<T, TId>` (`IReadRepository` + `IWriteRepository`), `IUnitOfWork`, `QueryOptions` |
| `Kck.Persistence.EntityFramework` | EF Core 10: `EfRepository<T, TId>`, `EfUnitOfWork`, `IEfRepositoryFactory`, interceptor'lar, `ToPaginateAsync` |

## Setup

```csharp
services.AddKckPersistence(p => p
    .UseEntityFramework<AppDbContext>(o => o
        .UseSqlServer(builder.Configuration.GetConnectionString("Default")))
    .AddAuditInterceptor());   // CreatedDate / ModifiedDate / soft-delete alanlari
```

`UseEntityFramework` DbContext'i, `IUnitOfWork`'u (scoped) ve `IEfRepositoryFactory`'yi kaydeder. `AddAuditInterceptor()`
ve `AddDomainEventDispatch<TDispatcher>()` ile kaydedilen interceptor'lar DbContext'e otomatik baglanir (4.1.0+);
ayni tipte bir interceptor `configure` icinde `AddInterceptors(...)` ile zaten eklenmisse ikinci kez eklenmez
(`OnConfiguring`'de eklenen gorulmez).

## Repository kullanimi

Repository'ler `IUnitOfWork` uzerinden alinir:

```csharp
public sealed class OrderService(IUnitOfWork uow)
{
    public Task<Order?> GetAsync(Guid id, CancellationToken ct) =>
        uow.Repository<Order, Guid>().GetByIdAsync(id, cancellationToken: ct);

    public async Task<Order> CreateAsync(Order order, CancellationToken ct)
    {
        await uow.Repository<Order, Guid>().AddAsync(order, ct);
        await uow.SaveChangesAsync(cancellationToken: ct);
        return order;
    }
}
```

## QueryOptions

`QueryOptions` soft-delete edilmis satirlari ve change tracking'i kontrol eder. Okuma metotlari `QueryOptions?` alir;
verilmezse `GetAsync`, `GetListAsync`, `GetListByDynamicAsync` tracking yapar, `AnyAsync`, `GetByIdAsync`, `CountAsync`
yapmaz. Hazir degerler: `QueryOptions.None`, `QueryOptions.Tracking`, `QueryOptions.WithDeleted`.

```csharp
var repo = uow.Repository<Order, Guid>();

var page     = await repo.GetListAsync(QueryOptions.None, predicate: o => o.Total > 100, size: 20);
var archived = await repo.GetListAsync(QueryOptions.WithDeleted, predicate: o => o.IsDeleted);
var exists   = await repo.AnyAsync(predicate: o => o.CustomerId == customerId);

// Soft-delete edilmis bir kaydi yukleyip degistirmek (tracking acik olmali):
var order = await repo.GetAsync(o => o.Id == id, new QueryOptions(IncludeDeleted: true, AsTracking: true));
```

> `QueryOptions.WithDeleted` tracking yapmaz. Yukle-degistir-`SaveChanges` akisinda
> `new QueryOptions(IncludeDeleted: true, AsTracking: true)` kullanin.

## Paging

`GetListAsync` / `GetListByDynamicAsync` `IPaginate<T>` doner (`Items`, `Index`, `Size`, `Count`, `Pages`). Kendi
sorgularinizda `IQueryable<T>.ToPaginateAsync(index, size)` kullanin:

```csharp
IPaginate<Order> page = await repo.Query()   // soft-delete filtreli, tracking kapali
    .OrderByDescending(o => o.CreatedDate)
    .ToPaginateAsync(index: 0, size: 20, cancellationToken: ct);
```

## Dinamik sorgu

`GetListByDynamicAsync(DynamicQuery)` yalniz `IFilterPropertyWhitelist<T>` kayitli entity'ler icin calisir; izin
listesinde olmayan alana filtre `ArgumentException` firlatir.

## Transaction

```csharp
await uow.BeginTransactionAsync(ct);
try
{
    // ... birden cok repository uzerinden islem
    await uow.SaveChangesAsync(cancellationToken: ct);
}
catch
{
    await uow.RollbackAsync(ct);
    throw;
}

await uow.CommitAsync(ct); // commit, ardindan transaction icinde kaydedilen domain event'leri dagitilir
```

Commit `try` disindadir: commit'ten sonra dagitim hata verirse veri zaten kalicidir, `RollbackAsync` cagrilmamalidir.

## Interceptor'lar

- `AuditInterceptor` — `IAuditable` entity'lerde `CreatedDate` / `ModifiedDate`; `ISoftDeletable` silmelerini
  `IsDeleted = true` + `DeletedDate`'e cevirir. Repository'de `permanent: true` ile yapilan silmeler kalici kalir.
  Soft-delete edilen satirlar yalniz repository sorgularindan ve soft-delete query filter'i uygulanan context'lerden
  gizlenir; unique index'ler silinmis satirlari disarida birakmiyorsa yeniden ekleme reddedilebilir.
- `DomainEventDispatchInterceptor` — `AddDomainEventDispatch<TDispatcher>()` ile kaydedilir; aggregate'lerin domain
  event'lerini `SaveChanges` sonrasi dagitir. 4.2.0+ davranisi:
  - EF'in `BeginTransaction`'i ile acilan bir transaction icinde kaydedilen event'ler o transaction'a baglanir;
    rollback, hata veya commit edilmeden dispose edilirse atilir. Commit'ten sonra `IUnitOfWork.CommitAsync` /
    `SaveChangesAsync(autoCommitTransaction: true)` onlari commit tamamlaninca dagitir; handler ayni DbContext ile
    kaydedebilir. Transaction `Database.BeginTransaction` ile acilip dogrudan commit edildiyse event'ler context'in
    sonraki `SaveChanges`'inde dagitilir; o context'te baska `SaveChanges` cagrilmazsa event'ler sessizce kaybolur —
    transaction'lari `IUnitOfWork` ile commit edin.
  - Dispatcher hata verirse veri kaydedilmis olur, hata cagirana doner ve event'ler entity'lere geri eklenir; ayni
    context'teki sonraki `SaveChanges` onlari yeniden dagitir. Teslim en-az-bir-kezdir: kismen basarili bir dagitim
    tekrarlanabilir, handler'lar idempotent olmalidir. Hata istegin/scope'un sonunu getirirse (web istegi, Mediator
    pipeline) ya da uygulama coker ise event'ler kaybolur — dayanikli teslim icin outbox gerekir.
  - Kapsam disi: `Database.UseTransaction` ile verilen dis transaction'in dogrudan commit'i (event'ler atilir),
    `TransactionScope`, savepoint'e geri donus (savepoint sonrasi event'ler commit'te yine dagitilir).
  - `SaveChangesAsync` tercih edin; senkron `SaveChanges` dagitim boyunca bir thread-pool thread'ini bloklar.
    DbContext pooling / `AddDbContextFactory` ile desteklenmez.

## Migrasyon

```bash
dotnet ef migrations add InitialCreate --project src/YourApp.Infrastructure
dotnet ef database update
```

`.config/dotnet-tools.json` dosyasinda `dotnet-ef` araci mevcut.
