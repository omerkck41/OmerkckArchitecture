# Kck.Http.Abstractions

Resilient HTTP client abstractions with typed `ApiResponse<T>` wrapper — decouple application code from `HttpClient` implementation details.

## Installation

```bash
dotnet add package Kck.Http.Abstractions
```

## Quick Start

```csharp
using Kck.Http.Abstractions;

// Program.cs — the provider (Kck.Http.Resilience): retry, circuit breaker and timeouts
builder.Services.AddKckHttpResilience(opt => opt.MaxRetryAttempts = 3);

public sealed record ChargeRequest(string Token, decimal Amount);
public sealed record PaymentResult(string PaymentId, string Status);

// Use IApiClient in a service (absolute URLs: the client has no base address)
public class PaymentGatewayService(IApiClient apiClient)
{
    private const string BaseUrl = "https://api.payment-gateway.example";

    public async Task<PaymentResult> ChargeAsync(ChargeRequest request, CancellationToken ct)
    {
        ApiResponse<PaymentResult> response = await apiClient.PostAsync<PaymentResult>($"{BaseUrl}/v1/charge", request, ct);
        if (!response.IsSuccess)
            throw new InvalidOperationException($"Payment failed ({response.StatusCode}): {response.ErrorMessage}");
        return response.Data!;
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Http:TimeoutSeconds` | Per-request timeout | `30` |
| `Http:RetryCount` | Polly retry attempts on transient failures | `3` |
| `Http:CircuitBreakerThreshold` | Failures before circuit opens | `5` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/http.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
