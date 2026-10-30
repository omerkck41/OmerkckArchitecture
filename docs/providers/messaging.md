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
    opt.UseSsl = true;
    opt.UserName = builder.Configuration["Mail:User"]!;
    opt.Password = builder.Configuration["Mail:Password"]!;
    opt.PoolSize = 5;
});
```

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
`SecretKey` can be left empty.

## Sending

```csharp
public class OrderConfirmationService(IEmailProvider email)
{
    public Task SendAsync(Order order, CancellationToken ct)
    {
        return email.SendAsync(new EmailMessage
        {
            From = "noreply@example.com",
            FromName = "MyApp",
            Recipients =
            [
                new EmailRecipient(order.CustomerEmail, order.CustomerName, RecipientType.To)
            ],
            Subject = $"Order #{order.Id} confirmed",
            Body = "<h1>Thanks for your purchase</h1>",
            IsHtml = true,
            Attachments =
            [
                new EmailAttachment("receipt.pdf", receiptStream, "application/pdf")
            ]
        }, ct);
    }
}
```

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
- The outbox provider was marked optional in PHASE 4. If it is added, it will
  integrate through the `IEmailOutbox` abstraction.
