# Kck.Messaging.AmazonSes

Amazon SES-backed `IEmailService` for transactional email delivery with template and raw message support.

## Installation

```bash
dotnet add package Kck.Messaging.AmazonSes
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckMessagingAmazonSes(options =>
{
    options.Region = "eu-central-1";
    options.AccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY")!;
    options.SecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY")!;
    options.FromAddress = "noreply@myapp.com";
});

// Send an email
public class NotificationService(IEmailService email)
{
    public async Task SendWelcomeAsync(string to, CancellationToken ct)
        => await email.SendAsync(to, "Welcome!", "<h1>Welcome!</h1>", ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Region` | AWS region for SES | required |
| `AccessKey` | AWS access key ID | required (or IAM role) |
| `SecretKey` | AWS secret access key | required (or IAM role) |
| `FromAddress` | Default sender address | required |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
