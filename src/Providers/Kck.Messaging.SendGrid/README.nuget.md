# Kck.Messaging.SendGrid

SendGrid API-backed `IEmailProvider` for reliable transactional and marketing email delivery at scale.

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
| `ApiKey` | SendGrid API key | required unless an `ISendGridClient` is registered |

The sender is set per message (`EmailMessage.From`, `FromName`). A response outside 2xx throws
`InvalidOperationException`. Attachment streams are read from the start (a seekable stream is rewound first).

When the container has an `ISendGridClient` registered, the provider uses it instead of building a client from
`ApiKey`; register your own client for custom `SendGridClientOptions` (proxy, host, EU data residency).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
