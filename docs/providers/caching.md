# Caching

`ICacheService` abstraction'i key-value cache icin unifiye edilmis arayuzdur.
Cache stampede koruma (`GetOrSetAsync`), prefix bazli toplu silme ve TTL destegi
tum provider'larda ortaktır.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Caching.Abstractions` | `ICacheService`, `CacheOptions`, `CacheServiceBase` |
| `Kck.Caching.InMemory` | `Microsoft.Extensions.Caching.Memory` |
| `Kck.Caching.Redis` | StackExchange.Redis |

## InMemory

Tek-process senaryolar icin. Distributed degildir.

```csharp
services.AddKckCachingInMemory(opt =>
{
    opt.KeyPrefix = "myapp:";
    opt.DefaultTtl = TimeSpan.FromMinutes(5);
});
```

## Redis

Distributed cache icin. Redis connection `IHostedService` olarak acilir
([ADR-0006](../adr/0006-redis-async-hosted-service.md)).

```csharp
services.AddKckCachingRedis(opt =>
{
    opt.ConnectionString = builder.Configuration["Redis:ConnectionString"]!;
    opt.KeyPrefix = "myapp:";
    opt.DefaultTtl = TimeSpan.FromMinutes(10);
});
```

## Kullanim

```csharp
public class ProductService(ICacheService cache)
{
    public async Task<Product> GetAsync(int id)
    {
        return await cache.GetOrSetAsync(
            $"product:{id}",
            async () => await _repo.FindAsync(id),
            TimeSpan.FromMinutes(10));
    }

    public async Task InvalidateAllAsync()
    {
        await cache.RemoveByPrefixAsync("product:");
    }
}
```

## API

| Metod | Aciklama |
|---|---|
| `GetAsync<T>(key)` | Null doner eger yoksa |
| `SetAsync<T>(key, value, ttl?)` | Opsiyonel TTL |
| `GetOrSetAsync<T>(key, factory, ttl?)` | Cache stampede korumali |
| `RemoveAsync(key)` | Tekil silme |
| `RemoveByPrefixAsync(prefix)` | Toplu silme (Redis: 1000/batch chunked) |
| `ExistsAsync(key)` | Bool check |

## Performans Notlari

- Redis `RemoveByPrefixAsync`: 1000 key/batch chunked — buyuk prefix silme
  tek-call'a sigmayacak tuple'larda bile calisir
- InMemory `GetOrSetAsync`: `SemaphoreSlim` ile stampede korumasi
- Redis `KeyPrefix` bos birakilirsa: `myapp:products:1` → `products:1` olur

## Secim Kriterleri

| Kriter | InMemory | Redis |
|---|---|---|
| Scope | Tek process | Cluster |
| Latency | < 1 ms | 1-5 ms |
| Persistence | Yok | Opsiyonel (RDB/AOF) |
| Use case | Unit test, CLI, single-instance | Web app, microservice |
