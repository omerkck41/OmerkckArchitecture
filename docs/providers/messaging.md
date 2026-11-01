# Messaging

A provider-agnostic `IEmailProvider` abstraction for sending email.
Attachments, HTML/text bodies and CC/BCC are supported as standard.

## Packages

| Package | Target |
|---|---|
| `Kck.Messaging.Abstractions` | `IEmailProvider`, `EmailMessage`, `EmailRecipient`, `EmailAttachment` |
| `Kck.Messaging.MailKit` | Over SMTP (MailKit) |
| `Kck.Messaging.SendGrid` | SendGrid API |
| `Kck.Messaging.AmazonSes` | Amazon SES v2 |

## MailKit (SMTP)

```csharp
services.AddKckMessagingMailKit(opt =>
{
    opt.Host = "smtp.gmail.com";
    opt.Port = 587;
    opt.UserName = builder.Configuration["Mail:User"]!;
    opt.Password = builder.Configuration["Mail:Password"]!;
    opt.PoolSize = 5;
});
```

The TLS mode follows the port: implicit TLS on 465 and mandatory STARTTLS on every other port (587, the default,
included), so the connection never falls back to plaintext. An explicit `SecureSocketOptions` of `None`, `Auto` or
`StartTlsWhenAvailable` opts out of that guarantee; with `None`, a `UserName` fails validation, because the
credentials would be sent in plaintext. Set `SecureSocketOptions` (namespace `MailKit.Security`)
to choose it explicitly, for example `SecureSocketOptions.SslOnConnect` for implicit TLS on a custom port; `UseSsl` is
consulted only while `SecureSocketOptions` is `null`, and `UseSsl = false` means opportunistic STARTTLS (used only when
the server offers it).

It uses an SMTP connection pool, because the `SmtpClient` implementation is not
thread-safe and is shared. `PoolSize` is typically equal to the number of
concurrent send workers.

## SendGrid

```csharp
services.AddKckMessagingSendGrid(opt =>
{
    opt.ApiKey = builder.Configuration["SendGrid:ApiKey"]!;
});
```

## Amazon SES

```csharp
services.AddKckMessagingAmazonSes(opt =>
{
    opt.Region = "eu-west-1";
    // Optional — if omitted, the default AWS credential chain is used
    opt.AccessKey = builder.Configuration["AWS:AccessKey"];
    opt.SecretKey = builder.Configuration["AWS:SecretKey"];
});
```

If `AWS_PROFILE` or an IAM role is set in the environment, `AccessKey` and
`SecretKey` can be left empty. When the container already has an `IAmazonSimpleEmailServiceV2` registered, the
provider uses that client instead of building one from the options (`Region` may then stay empty); register your
own client to control the credentials or the endpoint.

Attachments are sent through SESv2 `Simple.Attachments` (file name, MIME type and content, disposition attachment);
SES limits the whole message, attachments included, to 40 MB. Attachment streams are read from the start (a seekable
stream is rewound first). Each `SendAsync` sends one message, so there is no bulk send and no partial success to
handle. `FromName` and recipient names are sent as friendly
addresses (`"Name" <user@example.com>`); a name with non-ASCII characters is sent RFC 2047 encoded
(`=?utf-8?B?...?= <user@example.com>`), as SES requires.

## Sending

```csharp
using Kck.Messaging.Abstractions;

public class OrderConfirmationService(IEmailProvider email)
{
    public Task SendAsync(Order order, Stream receiptPdf, CancellationToken ct) =>
        email.SendAsync(new EmailMessage
        {
            From = "noreply@example.com",
            FromName = "MyApp",
            Recipients = [new EmailRecipient(order.CustomerEmail, Type: RecipientType.To)],
            Subject = $"Order #{order.Id} confirmed",
            Body = "<h1>Thanks for your purchase</h1>",
            IsHtml = true,
            Attachments =
            [
                new EmailAttachment { FileName = "receipt.pdf", Content = receiptPdf, ContentType = "application/pdf" }
            ]
        }, ct);
}
```

Each provider registers `IEmailProvider`. (`IEmailService` has the same `SendAsync` shape but no implementation; it
is obsolete since 4.8.0, KCK0600.)

## Selection Criteria

| Criterion | MailKit | SendGrid | AmazonSes |
|---|---|---|---|
| Setup | SMTP server required | API key | AWS account |
| Cost | Self-hosted / Gmail / Office365 | Per-email tier | Per-email (cheap at scale) |
| Deliverability | Depends on your SMTP | High (managed IP) | High (managed) |
| Use case | Development, low volume | Marketing + transactional | High volume, AWS stack |

## Retry + Outbox

- Providers **do not implement retry**. This is the caller's responsibility.
- For critical emails, the outbox pattern is recommended (write to the database,
  send with a background job).
- There is no built-in email outbox.
