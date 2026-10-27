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
        .UseSqlServer(builder.Configuration.GetConnectionString("Default"))
        .AddInterceptors(new AuditInterceptor()))   // CreatedDate / UpdatedDate / soft-delete alanlari
    .AddAuditInterceptor());
```

`UseEntityFramework` DbContext'i, `IUnitOfWork`'u (scoped) ve `IEfRepositoryFactory`'yi kaydeder. Interceptor'lar
DbContext'e `AddInterceptors(...)` ile baglanir.

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
    await uow.SaveChangesAsync(autoCommitTransaction: true, cancellationToken: ct);
}
catch
{
    await uow.RollbackAsync(ct);
    throw;
}
```

## Interceptor'lar

- `AuditInterceptor` — `IAuditable` entity'lerde `CreatedDate` / `UpdatedDate`; `ISoftDeletable` silmelerini
  `IsDeleted = true` + `DeletedDate`'e cevirir.
- `DomainEventDispatchInterceptor` — `AddDomainEventDispatch<TDispatcher>()` ile kaydedilir; aggregate'lerin domain
  event'lerini `SaveChanges` sonrasi dagitir.

## Migrasyon

```bash
dotnet ef migrations add InitialCreate --project src/YourApp.Infrastructure
dotnet ef database update
```

`.config/dotnet-tools.json` dosyasinda `dotnet-ef` araci mevcut.
