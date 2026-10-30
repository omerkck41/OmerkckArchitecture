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
public class CatalogService(ICacheService cache, AppDbContext db)
{
    public async Task<List<Category>?> GetCategoriesAsync(CancellationToken ct)
        => await cache.GetOrSetAsync("categories",
               () => db.Categories.ToListAsync(ct),
               ct: ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `KeyPrefix` | Prefix prepended to every cache key | `null` |
| `DefaultExpiration` | Expiration when a call passes none | 5 minutes |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/caching.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
