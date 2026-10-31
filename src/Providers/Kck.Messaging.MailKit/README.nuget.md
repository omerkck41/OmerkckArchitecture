# Kck.Messaging.MailKit

MailKit SMTP-backed `IEmailProvider` for sending transactional emails via any SMTP server with TLS/STARTTLS support.

## Installation

```bash
dotnet add package Kck.Messaging.MailKit
```

## Quick Start

```csharp
using Kck.Messaging.Abstractions;

// Program.cs
builder.Services.AddKckMessagingMailKit(options =>
{
    options.Host = "smtp.example.com";
    options.Port = 587;
    options.UseSsl = true;
    options.UserName = Environment.GetEnvironmentVariable("SMTP_USER");
    options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
});

// Send through IEmailProvider (the sender address is part of each message)
public class PasswordResetMailer(IEmailProvider email)
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
| `Host` | SMTP server | required |
| `Port` | SMTP port | `587` |
| `UseSsl` | Require SSL/TLS | `true` |
| `UserName` | SMTP user; `null` for an unauthenticated relay | `null` |
| `Password` | SMTP password | `null` |
| `PoolSize` | SMTP connections kept in the pool | `5` |

The sender is set per message (`EmailMessage.From`).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
