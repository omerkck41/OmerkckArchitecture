# Kck.Messaging.AmazonSes

Amazon SES-backed `IEmailProvider` for transactional email delivery with attachments and display names (SESv2 `Simple` content).

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
| `Region` | AWS region of the SES endpoint | required unless an `IAmazonSimpleEmailServiceV2` is registered |
| `AccessKey` | AWS access key; omit to use the default credential chain | `null` |
| `SecretKey` | AWS secret key paired with `AccessKey` | `null` |

The sender is set per message (`EmailMessage.From`). `FromName` and recipient names are sent as friendly addresses
(`"Name" <user@example.com>`); a name with non-ASCII characters is sent RFC 2047 encoded (`=?utf-8?B?...?=`).

Attachments are sent through SESv2 `Simple.Attachments` (file name, MIME type and content, disposition attachment);
SES limits the whole message, attachments included, to 40 MB. Attachment streams are read from the start (a seekable
stream is rewound first). Each `SendAsync` sends one message: there is no bulk send and no partial success.

When the container has an `IAmazonSimpleEmailServiceV2` registered, the provider uses it instead of building a client
from the options; register your own client to control the credentials or the endpoint.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
