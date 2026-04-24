# Messaging

E-posta gonderimi icin provider-agnostic `IEmailProvider` abstraction.
Attachment, HTML/text body, CC/BCC destegi standart.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Messaging.Abstractions` | `IEmailProvider`, `EmailMessage`, `EmailRecipient`, `EmailAttachment` |
| `Kck.Messaging.MailKit` | SMTP uzerinden (MailKit) |
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

SMTP connection pool'u kullanir — thread-safe olmayan `SmtpClient`
implementasyon'i paylasimli. `PoolSize` tipik olarak concurrent send worker
sayisi kadardir.

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
    // Opsiyonel — birakilirsa default AWS credential chain
    opt.AccessKey = builder.Configuration["AWS:AccessKey"];
    opt.SecretKey = builder.Configuration["AWS:SecretKey"];
});
```

Environment'da `AWS_PROFILE` veya IAM role set ise `AccessKey`/`SecretKey`
bos birakilabilir.

## Gonderim

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

## Secim Kriterleri

| Kriter | MailKit | SendGrid | AmazonSes |
|---|---|---|---|
| Kurulum | SMTP server gerekli | API key | AWS account |
| Cost | Self-hosted / Gmail / Office365 | Per-email tier | Per-email (cheap at scale) |
| Deliverability | Ozel SMTP'ye bagli | Yuksek (managed IP) | Yuksek (managed) |
| Use case | Development, dusuk hacim | Marketing + transactional | Yuksek hacim, AWS stack |

## Retry + Outbox

- Provider'lar **retry implement etmez** — bu sorumluluk caller'da
- Kritik e-postalar icin outbox pattern onerilir (veritabanina yaz, background
  job ile gonder)
- FAZ 4'te outbox provider opsiyonel olarak isaretlendi — eklenirse
  `IEmailOutbox` abstraction'i uzerinden entegre olacak
