# Kck.Messaging.SendGrid

SendGrid API-backed `IEmailService` for reliable transactional and marketing email delivery at scale.

## Installation

```bash
dotnet add package Kck.Messaging.SendGrid
```

## Quick Start

```csharp
using Kck.Messaging.Abstractions;

// Program.cs
builder.Services.AddKckMessagingSendGrid(options =>
    options.ApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")!);

// Send through IEmailProvider (the sender address is part of each message)
public class InvoiceMailer(IEmailProvider email)
{
    public Task SendAsync(string to, string subject, string html, CancellationToken ct) =>
        email.SendAsync(new EmailMessage
        {
            From = "noreply@myapp.com",
            FromName = "My App",
            Recipients = [new EmailRecipient(to)],
            Subject = subject,
            Body = html,
            IsHtml = true
        }, ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ApiKey` | SendGrid API key | required |

The sender is set per message (`EmailMessage.From`, `FromName`).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
