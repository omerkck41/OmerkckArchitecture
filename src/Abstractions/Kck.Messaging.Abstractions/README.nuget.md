# Kck.Messaging.Abstractions

Email messaging abstractions for transactional email delivery — `IEmailService` with `EmailMessage`, `EmailRecipient`, and `EmailAttachment` models compatible with MailKit, SendGrid, or Amazon SES.

## Installation

```bash
dotnet add package Kck.Messaging.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider (e.g. Kck.Messaging.MailKit)
builder.Services.AddKckMailKitEmail(builder.Configuration);

// Use IEmailService in a handler
public class OrderConfirmationHandler(IEmailService email)
{
    public async Task SendConfirmationAsync(Order order, CancellationToken ct)
    {
        var message = new EmailMessage
        {
            To      = [new EmailRecipient(order.CustomerEmail, order.CustomerName)],
            Subject = $"Order #{order.Id} Confirmed",
            Body    = $"<h1>Thank you!</h1><p>Your order total: {order.Total:C}</p>",
            IsHtml  = true,
            Attachments =
            [
                new EmailAttachment
                {
                    FileName    = "invoice.pdf",
                    Content     = await GenerateInvoiceAsync(order, ct),
                    ContentType = "application/pdf"
                }
            ]
        };

        await email.SendAsync(message, ct);
    }
}
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
