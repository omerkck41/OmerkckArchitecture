# Kck.Http.Abstractions

Resilient HTTP client abstractions with typed `ApiResponse<T>` wrapper — decouple application code from `HttpClient` implementation details.

## Installation

```bash
dotnet add package Kck.Http.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider (e.g. Kck.Http.Resilience)
builder.Services.AddKckResilientHttpClient(builder.Configuration);

// Use IApiClient in a service
public class PaymentGatewayService(IApiClient apiClient)
{
    private const string BaseUrl = "https://api.payment-gateway.com";

    public async Task<PaymentResult?> ChargeAsync(
        ChargeRequest request, CancellationToken ct)
    {
        var response = await apiClient.PostAsync<ChargeRequest, PaymentResult>(
            $"{BaseUrl}/v1/charge", request, ct);

        if (!response.IsSuccess)
            throw new BadRequestException($"Payment failed: {response.ErrorMessage}");

        return response.Data;
    }

    public async Task<PaymentStatus?> GetStatusAsync(string paymentId, CancellationToken ct)
    {
        var response = await apiClient.GetAsync<PaymentStatus>(
            $"{BaseUrl}/v1/payments/{paymentId}", ct);
        return response.Data;
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
