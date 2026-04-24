# HTTP Resilience

Dis servis cagrilari icin retry + circuit breaker + timeout sargısı.
Polly (Microsoft.Extensions.Http.Resilience) uzerinde kurulu.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Http.Abstractions` | `IApiClient`, `ApiResponse<T>` |
| `Kck.Http.Resilience` | Polly tabanli `ResilientApiClient` |

## Setup

```csharp
services.AddKckHttpResilience(opt =>
{
    opt.BaseAddress = "https://api.upstream.com";
    opt.Timeout = TimeSpan.FromSeconds(30);
    opt.MaxRetries = 3;
    opt.CircuitBreakerFailureThreshold = 5;
    opt.CircuitBreakerDurationOfBreak = TimeSpan.FromSeconds(30);
});
```

## Kullanim

```csharp
public class UpstreamService(IApiClient client)
{
    public async Task<ApiResponse<Product>> GetProductAsync(int id, CancellationToken ct)
    {
        return await client.GetAsync<Product>($"/products/{id}", ct);
    }

    public async Task<ApiResponse<Order>> PostOrderAsync(Order order, CancellationToken ct)
    {
        return await client.PostAsync<Order, Order>("/orders", order, ct);
    }
}
```

## Resilience Policies

Provider asagidaki policy'leri kademeli uygular:

| Policy | Davranis |
|---|---|
| Timeout | Her istek icin `Timeout` (varsayilan 30s) |
| Retry | Transient hata (5xx, timeout) uzerine `MaxRetries` kez exponential backoff |
| Circuit Breaker | Ardisik basarisizliktan sonra `CircuitBreakerDurationOfBreak` kadar istek reddedilir |

## ApiResponse\<T>

```csharp
public record ApiResponse<T>(
    bool IsSuccess,
    T? Data,
    string? ErrorMessage,
    int StatusCode);
```

- Http 2xx → `IsSuccess = true`, `Data` dolu
- Http 4xx/5xx (retry sonrasi) → `IsSuccess = false`, `ErrorMessage` dolu
- Circuit breaker acikken → `IsSuccess = false`, status 503

## Logging

Retry denemelerini `ILogger` ile loglar. Detay attempt sayisi + reason.

## Multi-Tenant

Birden fazla upstream servis icin `IHttpClientFactory` named client pattern
kullanin — her biri icin ayri `AddKckHttpResilience()` cagrisi ile
`TypedClient` kaydedin.
