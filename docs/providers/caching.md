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
| `Kck.Caching.Hybrid` | `Microsoft.Extensions.Caching.Hybrid`: in-process L1 over Redis L2 |

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

## Hybrid

An in-process L1 cache in front of Redis (L2): reads are served from memory while the entry is fresh there, and
every instance shares the Redis copy. Redis is required: `ExistsAsync` and `RemoveByPrefixAsync` go to Redis, so
there is no Redis-less mode. Without Redis, use `Kck.Caching.InMemory` (a hybrid without L2 would be the same
cache). `RemoveAsync` and `RemoveByPrefixAsync` clear Redis and this instance's L1 only: other instances can serve
their L1 copy until `DefaultLocalExpiration`, so keep it short for data that changes. As with Redis, the connection is
opened asynchronously when the host starts and a missing Redis configuration fails at registration; resolving
`ICacheService` before the host's `StartAsync` completed throws. Do not register `AddKckCachingRedis` and
`AddKckCachingHybrid` in the same application: their two hosted services open two Redis connections, and the first
`IConnectionMultiplexer`/`ICacheService` registration wins. Setup and options: the package [README](https://github.com/omerkck41/OmerkckArchitecture/tree/main/src/Providers/Kck.Caching.Hybrid).

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

The reference app caches its product catalogue this way:
[`ListProducts.cs`](https://github.com/omerkck41/OmerkckArchitecture/blob/main/samples/Kck.Sample.Orders/Features/Products/ListProducts.cs).

## API

| Method | Description |
|---|---|
| `GetAsync<T>(key)` | Returns null if the key does not exist |
| `SetAsync<T>(key, value, expiration?)` | Without an expiration, `DefaultExpiration` (5 minutes) applies |
| `GetOrSetAsync<T>(key, factory, expiration?)` | Cache stampede protected; a cached `0`/`false`/`Guid.Empty` is a hit, a cached `null` is a miss (the factory runs again; Hybrid: a `null` result is cached) |
| `RemoveAsync(key)` | Single removal |
| `RemoveByPrefixAsync(prefix)` | Bulk removal (Redis: chunked, 1000 per batch) |
| `ExistsAsync(key)` | Boolean check |

## Performance Notes

- Redis `RemoveByPrefixAsync`: chunked at 1000 keys per batch, so removing a large
  prefix works even when the keys do not fit in a single call
- `GetOrSetAsync` (InMemory, Redis): stampede protection with a 64-stripe `SemaphoreSlim` array, so concurrent misses
  of one key run the factory once per process. Hybrid goes through `HybridCache.GetOrCreateAsync` instead: one factory
  run per key across callers, the result written to L1 and L2 (a `null` result is cached too, for the entry's lifetime)
- `CacheServiceBase` (your own provider): `GetOrSetAsync` decides a hit with `TryGetAsync<T>`, which returns
  `(bool Found, T? Value)`. The default implementation treats a `null` from `GetAsync` as a miss, which is wrong for
  value types; override it when the store can report presence (InMemory and Redis do)
- If the Redis `KeyPrefix` is left empty: `myapp:products:1` becomes `products:1`

## Selection Criteria

| Criterion | InMemory | Redis | Hybrid |
|---|---|---|---|
| Scope | Single process | Cluster | Cluster |
| Latency | < 1 ms | 1-5 ms | < 1 ms (L1 hit), 1-5 ms (L2) |
| Persistence | None | Optional (RDB/AOF) | As Redis |
| Use case | Unit test, CLI, single-instance | Web app, microservice | Hot, read-heavy keys on several instances |
