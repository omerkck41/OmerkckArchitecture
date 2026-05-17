# Kck.Caching.Abstractions

Storage-agnostic caching abstractions with stampede-safe `ICacheService.GetOrSetAsync` and prefix-based bulk invalidation.

## Installation

```bash
dotnet add package Kck.Caching.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider (e.g. Kck.Caching.InMemory or Kck.Caching.Redis)
builder.Services.AddKckInMemoryCache();
// or: builder.Services.AddKckRedisCache(builder.Configuration);

// Use ICacheService in a service/handler
public class ProductService(ICacheService cache)
{
    public async Task<Product?> GetProductAsync(int id, CancellationToken ct)
    {
        // Fetches from cache; on miss calls the factory and stores the result
        return await cache.GetOrSetAsync(
            key: $"product:{id}",
            factory: async () => await _db.Products.FindAsync(id, ct),
            ttl: TimeSpan.FromMinutes(10),
            ct: ct);
    }

    public async Task InvalidateCategoryAsync(int categoryId, CancellationToken ct)
    {
        // Remove all keys sharing the "category:{categoryId}" prefix
        await cache.RemoveByPrefixAsync($"category:{categoryId}", ct);
    }
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
