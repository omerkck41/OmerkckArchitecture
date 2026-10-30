# Kck.Caching.Redis

StackExchange.Redis-backed `ICacheService` for distributed caching with key-prefix namespacing and O(1) existence checks.

## Installation

```bash
dotnet add package Kck.Caching.Redis
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckCachingRedis(
    redis => redis.Configuration = "localhost:6379",   // or ConfigurationOptions / ConnectionMultiplexerFactory
    cache =>
    {
        cache.KeyPrefix = "myapp:";
        cache.DefaultExpiration = TimeSpan.FromMinutes(30);
    });

// Usage
public class ProductService(ICacheService cache, IProductRepository products)
{
    public Task<Product?> GetAsync(int id, CancellationToken ct) =>
        cache.GetOrSetAsync($"product:{id}", () => products.FindAsync(id, ct), TimeSpan.FromMinutes(10), ct);

    public Task InvalidateAllAsync(CancellationToken ct) => cache.RemoveByPrefixAsync("product:", ct);
}
```

## Configuration

First callback: `RedisCacheOptions` (Microsoft.Extensions.Caching.StackExchangeRedis); set `Configuration`, `ConfigurationOptions` or `ConnectionMultiplexerFactory`. Second callback:

| Property | Description | Default |
|---|---|---|
| `KeyPrefix` | Prefix prepended to every cache key | `null` |
| `DefaultExpiration` | Expiration when a call passes none | 5 minutes |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/caching.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
