# HTTP Resilience

A retry + circuit breaker + timeout wrapper for external service calls.
Built on Polly (Microsoft.Extensions.Http.Resilience).

## Packages

| Package | Target |
|---|---|
| `Kck.Http.Abstractions` | `IApiClient`, `ApiResponse<T>` |
| `Kck.Http.Resilience` | Polly-based `ResilientApiClient` |

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

## Usage

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

The provider applies the following policies in layers:

| Policy | Behavior |
|---|---|
| Timeout | `Timeout` for each request (default 30s) |
| Retry | On a transient error (5xx, timeout), retries `MaxRetries` times with exponential backoff |
| Circuit Breaker | After consecutive failures, requests are rejected for `CircuitBreakerDurationOfBreak` |

## ApiResponse\<T>

```csharp
public record ApiResponse<T>(
    bool IsSuccess,
    T? Data,
    string? ErrorMessage,
    int StatusCode);
```

- HTTP 2xx → `IsSuccess = true`, `Data` is populated
- HTTP 4xx/5xx (after retries) → `IsSuccess = false`, `ErrorMessage` is populated
- While the circuit breaker is open → `IsSuccess = false`, status 503

## Logging

Retry attempts are logged with `ILogger`. The log detail includes the attempt count and the reason.

## Multi-Tenant

For multiple upstream services, use the `IHttpClientFactory` named client
pattern. Register a `TypedClient` for each one with its own
`AddKckHttpResilience()` call.
