# Kck.Messaging.SendGrid

SendGrid API-backed `IEmailService` for reliable transactional and marketing email delivery at scale.

## Installation

```bash
dotnet add package Kck.Messaging.SendGrid
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckMessagingSendGrid(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")!;
    options.FromAddress = "noreply@myapp.com";
    options.FromName = "My App";
});

// Send email
public class InvoiceService(IEmailService email)
{
    public async Task SendInvoiceAsync(string to, string html, CancellationToken ct)
        => await email.SendAsync(to, "Your invoice", html, ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ApiKey` | SendGrid API key | required |
| `FromAddress` | Default sender email address | required |
| `FromName` | Display name for the sender | `null` |
| `SandboxMode` | Enable SendGrid sandbox (no actual delivery) | `false` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
