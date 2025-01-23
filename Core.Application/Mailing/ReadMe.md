Tabii, son güncellemelerle birlikte `ReadMe.md` dosyanızı güncelleyelim. Özellikle `IEmailRequest` arayüzü ve `EmailSendingBehavior`'ın genişletilmiş kullanımına odaklanarak, README dosyasını daha detaylı ve kullanıcı dostu hale getireceğim.

---

# Mail Gönderme Kütüphanesi

Bu kütüphane, .NET 9.0 üzerinde geliştirilmiş, esnek ve genişletilebilir bir mail gönderme çözümü sunar. Kütüphane, birden fazla mail sağlayıcısı (SMTP, SendGrid, Amazon SES vb.) ile çalışabilme özelliğine sahiptir ve rate limiting, logging, behavior'lar gibi özelliklerle desteklenmiştir. Ayrıca, builder pattern kullanılarak mail mesajlarının kolayca oluşturulmasını sağlar.

---

## Özellikler

- **Çoklu Sağlayıcı Desteği:** SMTP, SendGrid ve Amazon SES gibi birden fazla mail sağlayıcısı ile çalışabilme.
- **Rate Limiting:** SMTP istemcileri için rate limiting desteği.
- **Builder Pattern:** Mail mesajlarını kolayca oluşturmak için builder pattern kullanımı.
- **Logging:** E-posta gönderme işlemleri sırasında detaylı loglama.
- **Behavior'lar:** MediatR pipeline'ı üzerinden e-posta gönderme işlemlerini otomatikleştirme.
- **Async/Await:** Tüm işlemler asenkron olarak gerçekleştirilir.
- **Genişletilebilirlik:** Yeni mail sağlayıcıları ve behavior'lar kolayca eklenebilir.

---

## Kurulum

### 1. Projeye Ekleme

1. **Manuel Ekleme:** Eğer kütüphaneyi manuel olarak eklemek istiyorsanız, tüm sınıfları projenize kopyalayın ve gerekli bağımlılıkları ekleyin.

---

### 2. Yapılandırma

Kütüphaneyi kullanmadan önce, gerekli yapılandırmaları yapmanız gerekmektedir. Bu yapılandırmalar, `appsettings.json` dosyası üzerinden yapılabilir.

#### `appsettings.json` Örneği:

```json
{
  "EmailSettings": {
    "DefaultFromAddress": "noreply@rentacar.com",
    "DefaultFromName": "ArchonApp Support",
    "SendGridApiKey": "your-sendgrid-api-key",
    "Providers": {
      "Smtp": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "EnableSsl": true,
        "UserName": "your-email@gmail.com",
        "Password": "your-email-password",
        "MaxEmailsPerClient": 100
      }
    },
    "AmazonSes": {
      "AccessKeyId": "your-aws-access-key-id",
      "SecretAccessKey": "your-aws-secret-access-key",
      "Region": "us-east-1"
    }
  }
}
```

---

### 3. Dependency Injection

Kütüphaneyi kullanmak için gerekli servisleri `Startup.cs` veya `Program.cs` dosyasında Dependency Injection ile kaydedin.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // SMTP istemcilerini yapılandırma
    var smtpClients = new List<SmtpClient>
    {
        new SmtpClient("smtp.example.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential("your-username", "your-password")
        }
    };

    // RateLimitingSmtpClientSelector'ı kaydet
    services.AddSingleton<ISmtpClientSelector>(new RateLimitingSmtpClientSelector(smtpClients, 100));

    // Email sağlayıcılarını kaydet
    services.AddSingleton<IEmailProvider, SmtpEmailProvider>();
    services.AddSingleton<IEmailProvider, SendGridEmailProvider>();

    // Email gönderme servisini kaydet
    services.AddSingleton<IMailService, EmailSendingService>();

    // MediatR ve Behavior'ları kaydet
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EmailLoggingBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EmailSendingBehavior<,>));
}
```

---

## Kullanım

### 1. Mail Mesajı Oluşturma

Mail mesajı oluşturmak için `EmailMessageBuilder` sınıfını kullanabilirsiniz.

```csharp
var emailMessage = new EmailMessageBuilder()
    .AddFrom("sender@example.com")
    .AddRecipient("recipient@example.com", RecipientType.To, "Recipient Name")
    .AddSubject("Test Email")
    .AddBody("<h1>Hello World!</h1>", isHtml: true)
    .MarkAsImportant()
    .AddAttachment("path/to/file.pdf", "application/pdf")
    .Build();
```

---

### 2. Behavior'lar ile E-posta Gönderme

Behavior'lar, MediatR pipeline'ı üzerinden e-posta gönderme işlemlerini otomatikleştirir. Örneğin, bir request işlendiğinde otomatik olarak e-posta göndermek için `EmailSendingBehavior` kullanılabilir.

#### a. **`IEmailRequest` Arayüzü ile E-posta Gönderme**

`IEmailRequest` arayüzünü implemente eden herhangi bir request, e-posta gönderme işlemini otomatik olarak tetikleyebilir.

```csharp
public interface IEmailRequest
{
    EmailMessage GetEmailMessage();
}
```

#### b. **Örnek Request ve Handler:**

```csharp
public class SendWelcomeEmailRequest : IRequest, IEmailRequest
{
    public string Email { get; set; }
    public string Name { get; set; }

    public EmailMessage GetEmailMessage()
    {
        return new EmailMessageBuilder()
            .AddFrom("noreply@example.com")
            .AddRecipient(Email, RecipientType.To, Name)
            .AddSubject("Welcome to Our Platform!")
            .AddBody($"<h1>Hello {Name}, welcome to our platform!</h1>", isHtml: true)
            .MarkAsImportant()
            .Build();
    }
}

public class SendWelcomeEmailHandler : IRequestHandler<SendWelcomeEmailRequest>
{
    public async Task<Unit> Handle(SendWelcomeEmailRequest request, CancellationToken cancellationToken)
    {
        // E-posta gönderme işlemi behavior tarafından otomatik olarak yapılacak
        return Unit.Value;
    }
}
```

#### c. **Behavior'ların Çalışma Mantığı:**

1. **`EmailLoggingBehavior`:** Request ve response'ları loglar.
2. **`EmailSendingBehavior`:** Eğer request `IEmailRequest` arayüzünü implemente ediyorsa, otomatik olarak e-posta gönderir.

---

### 3. Manuel E-posta Gönderme

E-posta göndermek için `IMailService` arayüzünü kullanın.

```csharp
public class MyService
{
    private readonly IMailService _mailService;

    public MyService(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task SendEmailAsync()
    {
        var emailMessage = new EmailMessageBuilder()
            .AddFrom("sender@example.com")
            .AddRecipient("recipient@example.com", RecipientType.To, "Recipient Name")
            .AddSubject("Test Email")
            .AddBody("<h1>Hello World!</h1>", isHtml: true)
            .Build();

        await _mailService.SendEmailAsync(emailMessage);
    }
}
```

---

### 4. Birden Fazla Sağlayıcı ile Çalışma

Kütüphane, birden fazla mail sağlayıcısı ile çalışabilme özelliğine sahiptir. Eğer bir sağlayıcı başarısız olursa, bir sonraki sağlayıcı ile mail göndermeyi deneyecektir.

```csharp
public async Task SendEmailWithFallbackAsync()
{
    var emailMessage = new EmailMessageBuilder()
        .AddFrom("sender@example.com")
        .AddRecipient("recipient@example.com", RecipientType.To, "Recipient Name")
        .AddSubject("Test Email")
        .AddBody("<h1>Hello World!</h1>", isHtml: true)
        .Build();

    try
    {
        await _mailService.SendEmailAsync(emailMessage);
    }
    catch (InvalidOperationException ex)
    {
        // Tüm sağlayıcılar başarısız oldu
        Console.WriteLine("All email providers failed: " + ex.Message);
    }
}
```

---