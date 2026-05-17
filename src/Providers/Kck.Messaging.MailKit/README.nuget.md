# Kck.Messaging.MailKit

MailKit SMTP-backed `IEmailService` for sending transactional emails via any SMTP server with TLS/STARTTLS support.

## Installation

```bash
dotnet add package Kck.Messaging.MailKit
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckMessagingMailKit(options =>
{
    options.Host = "smtp.example.com";
    options.Port = 587;
    options.UseSsl = false; // STARTTLS
    options.UserName = Environment.GetEnvironmentVariable("SMTP_USER")!;
    options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")!;
    options.FromAddress = "noreply@myapp.com";
});

// Send email
public class PasswordService(IEmailService email)
{
    public async Task SendResetAsync(string to, string link, CancellationToken ct)
        => await email.SendAsync(to, "Reset your password", $"<a href='{link}'>Reset</a>", ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Host` | SMTP server hostname | required |
| `Port` | SMTP server port | `587` |
| `UseSsl` | Use implicit TLS (port 465) | `false` |
| `UserName` | SMTP authentication username | required |
| `Password` | SMTP authentication password | required |
| `FromAddress` | Default sender email address | required |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
