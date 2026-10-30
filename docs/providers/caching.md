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
    opt.DefaultTtl = TimeSpan.FromMinutes(5);
});
```

## Redis

For distributed caching. The Redis connection is opened as an `IHostedService`
([ADR-0006](../adr/0006-redis-async-hosted-service.md)).

```csharp
services.AddKckCachingRedis(opt =>
{
    opt.ConnectionString = builder.Configuration["Redis:ConnectionString"]!;
    opt.KeyPrefix = "myapp:";
    opt.DefaultTtl = TimeSpan.FromMinutes(10);
});
```

## Usage

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

| Method | Description |
|---|---|
| `GetAsync<T>(key)` | Returns null if the key does not exist |
| `SetAsync<T>(key, value, ttl?)` | Optional TTL |
| `GetOrSetAsync<T>(key, factory, ttl?)` | Cache stampede protected |
| `RemoveAsync(key)` | Single removal |
| `RemoveByPrefixAsync(prefix)` | Bulk removal (Redis: chunked, 1000 per batch) |
| `ExistsAsync(key)` | Boolean check |

## Performance Notes

- Redis `RemoveByPrefixAsync`: chunked at 1000 keys per batch, so removing a large
  prefix works even when the keys do not fit in a single call
- InMemory `GetOrSetAsync`: stampede protection with `SemaphoreSlim`
- If the Redis `KeyPrefix` is left empty: `myapp:products:1` becomes `products:1`

## Selection Criteria

| Criterion | InMemory | Redis |
|---|---|---|
| Scope | Single process | Cluster |
| Latency | < 1 ms | 1-5 ms |
| Persistence | None | Optional (RDB/AOF) |
| Use case | Unit test, CLI, single-instance | Web app, microservice |
