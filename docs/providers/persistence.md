# Persistence

Entity Framework Core tabanli repository + Unit of Work + query specification.
Domain event dispatch interceptor ve soft-delete query filter entegre.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Persistence.Abstractions` | `IRepository<T>`, `IUnitOfWork`, `ISpecification<T>`, `QueryOptions` |
| `Kck.Persistence.EntityFramework` | EF Core 10 + `EfRepository<T>`, `EfUnitOfWork`, `IEfRepositoryFactory` |

## Setup

```csharp
services.AddKckPersistenceEntityFramework<MyDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

## DbContext

```csharp
public class MyDbContext(DbContextOptions<MyDbContext> opts) : DbContext(opts)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Soft-delete, tenant filter vs. interceptor'larla eklenir
    }
}
```

## Repository Kullanimi

```csharp
public class ProductService(IRepository<Product> repo)
{
    public Task<Product?> GetAsync(int id, CancellationToken ct)
        => repo.FindAsync(id, ct);

    public Task<List<Product>> ListAsync(CancellationToken ct)
        => repo.ListAsync(ct);

    public Task AddAsync(Product p, CancellationToken ct)
        => repo.AddAsync(p, ct);

    public Task DeleteRangeAsync(IEnumerable<Product> items, CancellationToken ct)
        => repo.DeleteRangeAsync(items, ct); // Bulk, tek geciste
}
```

## Specification Pattern

```csharp
public sealed class ActiveProductsByCategory(int categoryId)
    : Specification<Product>
{
    public override IQueryable<Product> Apply(IQueryable<Product> q) =>
        q.Where(p => p.Active && p.CategoryId == categoryId)
         .OrderBy(p => p.Name);
}

var active = await repo.ListAsync(new ActiveProductsByCategory(5), ct);
```

## QueryOptions

Soft-delete'li kayitlari dahil etmek veya tracking kapatmak icin:

```csharp
var query = repo.Query(new QueryOptions
{
    IncludeDeleted = true,
    AsTracking = false
});
var archived = await query.Where(p => p.IsDeleted).ToListAsync(ct);
```

## UnitOfWork

Cok-repository transaction icin factory pattern
([ADR-0005](../adr/0005-ef-repository-factory.md)):

```csharp
public class CheckoutService(IEfRepositoryFactory factory, IUnitOfWork uow)
{
    public async Task HandleAsync(int orderId, CancellationToken ct)
    {
        var orders = factory.Create<Order>();
        var products = factory.Create<Product>();

        // ... iki repository uzerinden islem

        await uow.SaveChangesAsync(ct);
    }
}
```

## Interceptor'lar

Provider asagidakileri otomatik kaydeder:

- `AuditInterceptor` — `CreatedAt` / `UpdatedAt` / `CreatedBy` otomatik doldurur
- `SoftDeleteInterceptor` — `Delete()` yerine `IsDeleted = true` + global
  query filter
- `DomainEventDispatchInterceptor` — aggregate root'tan domain event'leri
  toplar, `SaveChanges` sonrasi `IEventBus`'a publish eder

## Migrasyon

```bash
dotnet ef migrations add InitialCreate --project src/Providers/Kck.Persistence.EntityFramework
dotnet ef database update
```

`.config/dotnet-tools.json` dosyasinda `dotnet-ef 10.0.0` araci mevcut.

## Paging

```csharp
Paginate<Product> page = await repo.PaginateAsync(
    pageIndex: 0, pageSize: 20, specification: spec, ct);

Console.WriteLine($"{page.Items.Count} of {page.TotalCount}");
```

`Paginate<T>.Create(items, total, page, size)` test fixture'larinda kullanilir
(internal sync ctor yerine).
