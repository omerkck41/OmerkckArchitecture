# Kck.Caching.Redis

StackExchange.Redis-backed `ICacheService` for distributed caching with key-prefix namespacing and O(1) existence checks.

## Installation

```bash
dotnet add package Kck.Caching.Redis
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckCachingRedis(options =>
{
    options.Configuration = "localhost:6379";
    options.KeyPrefix = "myapp";
    options.DefaultExpiration = TimeSpan.FromMinutes(30);
});

// Usage
public class OrderService(ICacheService cache)
{
    public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
        => await cache.GetOrSetAsync($"order:{id}",
               () => _db.Orders.FindAsync(id, ct).AsTask(),
               ct: ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Configuration` | Redis connection string or `ConfigurationOptions` | `"localhost:6379"` |
| `KeyPrefix` | Namespace prefix for all cache keys | `""` |
| `DefaultExpiration` | Default entry time-to-live | `TimeSpan.FromMinutes(30)` |
| `Database` | Redis database index | `0` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/caching.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
