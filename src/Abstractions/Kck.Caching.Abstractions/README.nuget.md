# Kck.Caching.Abstractions

Storage-agnostic caching abstractions with stampede-safe `ICacheService.GetOrSetAsync` and prefix-based bulk invalidation.

## Installation

```bash
dotnet add package Kck.Caching.Abstractions
```

## Quick Start

```csharp
// Program.cs - a provider: Kck.Caching.InMemory, Kck.Caching.Redis or Kck.Caching.Hybrid
builder.Services.AddKckCachingInMemory(opt => opt.KeyPrefix = "myapp:");
// or: builder.Services.AddKckCachingRedis(redis => redis.Configuration = "localhost:6379");

// Usage
public class ProductService(ICacheService cache, IProductRepository products)
{
    public Task<Product?> GetAsync(int id, CancellationToken ct) =>
        cache.GetOrSetAsync($"product:{id}", () => products.FindAsync(id, ct), TimeSpan.FromMinutes(10), ct);

    public Task InvalidateAllAsync(CancellationToken ct) => cache.RemoveByPrefixAsync("product:", ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Caching:DefaultTtlSeconds` | Default TTL when none is specified | `300` |
| `Caching:KeyPrefix` | Global key prefix (useful for multi-tenant) | `""` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/caching.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
