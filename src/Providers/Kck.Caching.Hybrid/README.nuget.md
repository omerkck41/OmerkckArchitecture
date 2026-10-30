# Kck.Caching.Hybrid

Two-level hybrid cache provider combining an in-process L1 memory cache with a distributed L2 Redis cache for optimal read performance.

## Installation

```bash
dotnet add package Kck.Caching.Hybrid
```

## Quick Start

```csharp
// Program.cs - L1 in memory, L2 Redis (Microsoft.Extensions.Caching.Hybrid)
builder.Services.AddKckCachingHybrid(
    redis => redis.Configuration = "localhost:6379",
    hybrid =>
    {
        hybrid.KeyPrefix = "myapp:";
        hybrid.DefaultExpiration = TimeSpan.FromMinutes(30);
        hybrid.DefaultLocalExpiration = TimeSpan.FromMinutes(5);
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

First callback: `RedisCacheOptions` for the L2 Redis cache (`Configuration`). Second callback:

| Property | Description | Default |
|---|---|---|
| `KeyPrefix` | Prefix prepended to every cache key | `null` |
| `DefaultExpiration` | L1 + L2 expiration | 5 minutes |
| `DefaultLocalExpiration` | L1 (in-memory) expiration, at most `DefaultExpiration` | 1 minute |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/caching.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
