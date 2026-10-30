# Kck.Messaging.AmazonSes

Amazon SES-backed `IEmailService` for transactional email delivery with template and raw message support.

## Installation

```bash
dotnet add package Kck.Messaging.AmazonSes
```

## Quick Start

```csharp
using Kck.Messaging.Abstractions;

// Program.cs
builder.Services.AddKckMessagingAmazonSes(options =>
{
    options.Region = "eu-central-1";
    // Leave AccessKey/SecretKey unset to use the default AWS credential chain (IAM role, profile, env).
});

// Send through IEmailProvider (the sender address is part of each message)
public class NotificationMailer(IEmailProvider email)
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
| `Region` | AWS region of the SES endpoint | required |
| `AccessKey` | AWS access key; omit to use the default credential chain | `null` |
| `SecretKey` | AWS secret key paired with `AccessKey` | `null` |

The sender is set per message (`EmailMessage.From`).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
