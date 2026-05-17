# Kck.Http.Resilience

Microsoft.Extensions.Http.Resilience-based HTTP resilience pipelines — retry, circuit breaker, and timeout for `HttpClient` registrations.

## Installation

```bash
dotnet add package Kck.Http.Resilience
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckResilience("payment-api", pipeline =>
{
    pipeline
        .AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(500),
            UseJitter = true
        })
        .AddTimeout(TimeSpan.FromSeconds(10));
});

builder.Services.AddHttpClient<IPaymentClient, PaymentClient>()
    .AddResiliencePipeline("payment-api");
```

## Configuration

| Property | Description | Default |
|---|---|---|
| Pipeline name | Identifies the pipeline for `AddResiliencePipeline(name)` | required |
| `MaxRetryAttempts` | Number of retry attempts | `3` |
| `Delay` | Base delay between retries | `500ms` |
| `TimeoutInterval` | Per-request timeout | `10s` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/http.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
