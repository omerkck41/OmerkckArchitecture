# Kck.Caching.InMemory

`IMemoryCache`-backed `ICacheService` implementation for single-node in-process caching with key-prefix namespacing.

## Installation

```bash
dotnet add package Kck.Caching.InMemory
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckCachingInMemory(options =>
{
    options.KeyPrefix = "myapp";
    options.DefaultExpiration = TimeSpan.FromMinutes(10);
});

// Usage
public class CatalogService(ICacheService cache)
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct)
        => await cache.GetOrSetAsync("categories",
               () => _db.Categories.ToListAsync(ct),
               ct: ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `KeyPrefix` | Namespace prefix prepended to every cache key | `""` |
| `DefaultExpiration` | Default absolute expiration for cached entries | `TimeSpan.FromMinutes(10)` |
| `SizeLimit` | Maximum number of cache entries (0 = unlimited) | `0` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/caching.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
