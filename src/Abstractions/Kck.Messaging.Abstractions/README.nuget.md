# Kck.Messaging.Abstractions

Email messaging abstractions for transactional email delivery — `IEmailProvider` with `EmailMessage`, `EmailRecipient`, and `EmailAttachment` models compatible with MailKit, SendGrid, or Amazon SES.

## Installation

```bash
dotnet add package Kck.Messaging.Abstractions
```

## Quick Start

```csharp
using Kck.Messaging.Abstractions;

// Program.cs — a provider (MailKit, SendGrid or AmazonSes); each registers IEmailProvider
builder.Services.AddKckMessagingMailKit(opt =>
{
    opt.Host = builder.Configuration["Smtp:Host"]!;
    opt.UserName = builder.Configuration["Smtp:User"];
    opt.Password = builder.Configuration["Smtp:Password"];
});

// Send through IEmailProvider
public class OrderConfirmationHandler(IEmailProvider email)
{
    public Task SendConfirmationAsync(Order order, Stream invoicePdf, CancellationToken ct) =>
        email.SendAsync(new EmailMessage
        {
            From = "shop@example.com",
            Recipients = [new EmailRecipient(order.CustomerEmail)],
            Subject = $"Order #{order.Id} confirmed",
            Body = $"<h1>Thank you!</h1><p>Your order total: {order.Total:C}</p>",
            IsHtml = true,
            Attachments =
            [
                new EmailAttachment { FileName = "invoice.pdf", Content = invoicePdf, ContentType = "application/pdf" }
            ]
        }, ct);
}

// IEmailService is obsolete (KCK0600) and has no implementation; use IEmailProvider.
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Messaging:Email:Host` | SMTP host or provider endpoint | — |
| `Messaging:Email:Port` | SMTP port | `587` |
| `Messaging:Email:FromAddress` | Default sender address | — |
| `Messaging:Email:FromName` | Default sender display name | — |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
