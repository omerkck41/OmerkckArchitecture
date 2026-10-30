# Caching

The `ICacheService` abstraction is a unified interface for key-value caching.
Cache stampede protection (`GetOrSetAsync`), prefix-based bulk removal, and TTL support
are common to all providers.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Caching.Abstractions` | `ICacheService`, `CacheOptions`, `CacheServiceBase` |
| `Kck.Caching.InMemory` | `Microsoft.Extensions.Caching.Memory` |
| `Kck.Caching.Redis` | StackExchange.Redis |

## InMemory

For single-process scenarios. It is not distributed.

```csharp
services.AddKckCachingInMemory(opt =>
{
    opt.KeyPrefix = "myapp:";
    opt.DefaultExpiration = TimeSpan.FromMinutes(5);
});
```

## Redis

For distributed caching. The Redis connection is opened as an `IHostedService`
([ADR-0006](../adr/0006-redis-async-hosted-service.md)). The first callback is
`Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions`: set `Configuration`, `ConfigurationOptions` or
`ConnectionMultiplexerFactory`; registration throws when none is set.

```csharp
services.AddKckCachingRedis(
    redis => redis.Configuration = builder.Configuration["Redis:ConnectionString"],
    cache =>
    {
        cache.KeyPrefix = "myapp:";
        cache.DefaultExpiration = TimeSpan.FromMinutes(10);
    });
```

## Usage

```csharp
public class ProductService(ICacheService cache, IProductRepository products)
{
    public Task<Product?> GetAsync(int id, CancellationToken ct) =>
        cache.GetOrSetAsync(
            $"product:{id}",
            () => products.FindAsync(id, ct),
            TimeSpan.FromMinutes(10),
            ct);

    public Task InvalidateAllAsync(CancellationToken ct) =>
        cache.RemoveByPrefixAsync("product:", ct);
}
```

## API

| Method | Description |
|---|---|
| `GetAsync<T>(key)` | Returns null if the key does not exist |
| `SetAsync<T>(key, value, expiration?)` | Without an expiration, `DefaultExpiration` (5 minutes) applies |
| `GetOrSetAsync<T>(key, factory, expiration?)` | Cache stampede protected |
| `RemoveAsync(key)` | Single removal |
| `RemoveByPrefixAsync(prefix)` | Bulk removal (Redis: chunked, 1000 per batch) |
| `ExistsAsync(key)` | Boolean check |

## Performance Notes

- Redis `RemoveByPrefixAsync`: chunked at 1000 keys per batch, so removing a large
  prefix works even when the keys do not fit in a single call
- `GetOrSetAsync` (both providers): stampede protection with a 64-stripe `SemaphoreSlim` array, so concurrent misses
  of one key run the factory once per process
- If the Redis `KeyPrefix` is left empty: `myapp:products:1` becomes `products:1`

## Selection Criteria

| Criterion | InMemory | Redis |
|---|---|---|
| Scope | Single process | Cluster |
| Latency | < 1 ms | 1-5 ms |
| Persistence | None | Optional (RDB/AOF) |
| Use case | Unit test, CLI, single-instance | Web app, microservice |
