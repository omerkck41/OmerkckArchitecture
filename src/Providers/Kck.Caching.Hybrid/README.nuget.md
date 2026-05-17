# Kck.Caching.Hybrid

Two-level hybrid cache provider combining an in-process L1 memory cache with a distributed L2 Redis cache for optimal read performance.

## Installation

```bash
dotnet add package Kck.Caching.Hybrid
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckCachingHybrid(options =>
{
    options.KeyPrefix = "myapp";
    options.DefaultExpiration = TimeSpan.FromMinutes(30);
    options.DefaultLocalExpiration = TimeSpan.FromMinutes(5);
    options.RedisConfiguration = "localhost:6379";
});

// Usage
public class ProductService(ICacheService cache)
{
    public async Task<Product?> GetAsync(int id, CancellationToken ct)
        => await cache.GetOrSetAsync($"product:{id}",
               () => _db.Products.FindAsync(id, ct).AsTask(),
               ct: ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `KeyPrefix` | Namespace prefix for all cache keys | `""` |
| `DefaultExpiration` | L2 (Redis) entry lifetime | `TimeSpan.FromMinutes(30)` |
| `DefaultLocalExpiration` | L1 (in-memory) entry lifetime | `TimeSpan.FromMinutes(5)` |
| `RedisConfiguration` | Redis connection string | `"localhost:6379"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/caching.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
