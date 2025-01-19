# Gelişmiş E-posta Servisi Kullanım Rehberi

Bu döküman, geliştirilen e-posta servisi yapısının nasıl kullanılacağını ve temel bileşenlerini açıklamaktadır. Bu yapı, birden fazla sağlayıcı desteği (SMTP, SendGrid vb.) sunar ve büyük ölçekli projelerde kolay entegrasyon sağlar. **Behavior** ve **Service** olarak kullanım örneklerini içerir.

---

## 1. Proje Yapısı

- **EmailMessage**: E-posta verilerini içeren temel model.
- **EmailMessageBuilder**: EmailMessage sınıfını oluşturmak için kullanılan yapılandırıcı sınıf.
- **EmailRecipient**: Alıcıların bilgilerini tutan model.
- **RecipientType**: Alıcı türlerini (To, Cc, Bcc) tanımlar.
- **IMailService**: E-posta gönderim servisi için arayüz.
- **IEmailProvider**: E-posta sağlayıcıları için ortak bir arayüz.
- **SmtpEmailProvider** ve **SendGridEmailProvider**: SMTP ve SendGrid üzerinden e-posta göndermek için kullanılan sağlayıcılar.
- **EmailSendingService**: Sağlayıcıları sırayla deneyerek e-posta gönderen servis.
- **Behavior Sınıfları**:
  - **EmailLoggingBehavior**: E-posta işlemlerini loglar.
  - **EmailSendingBehavior**: E-posta gönderimini MediatR pipeline'a dahil eder.

---

## 2. Projeye Entegrasyon

### 2.1 Gerekli Bağımlılıkların Yüklenmesi

```bash
# SMTP ve diğer sağlayıcılar için
Install-Package MailKit
Install-Package SendGrid
Install-Package Microsoft.Extensions.DependencyInjection
Install-Package MediatR.Extensions.Microsoft.DependencyInjection
```

---

### 2.2 Dependency Injection Yapılandırması

Projenizdeki `Startup.cs` ya da `Program.cs` dosyasına aşağıdaki kodları ekleyin:

```csharp
using Core.Application.Mailing.Services;

// SMTP Client Selector yapılandırması
services.AddSingleton<ISmtpClientSelector, RateLimitingSmtpClientSelector>(provider =>
{
    var smtpClients = new List<SmtpClient>
    {
        new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential("user@gmail.com", "password"),
            EnableSsl = true
        },
        new SmtpClient("smtp.office365.com", 587)
        {
            Credentials = new NetworkCredential("user@outlook.com", "password"),
            EnableSsl = true
        }
    };

    return new RateLimitingSmtpClientSelector(smtpClients);
});

// Sağlayıcıların kaydedilmesi
services.AddSingleton<IEmailProvider, SmtpEmailProvider>();
services.AddSingleton<IEmailProvider, SendGridEmailProvider>();

// Ana e-posta gönderim servisi
services.AddSingleton<IMailService, EmailSendingService>();

// Behavior'ların eklenmesi
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EmailLoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EmailSendingBehavior<,>));
```

---

## 3. Kullanım Örnekleri

### 3.1 E-posta Mesajı Hazırlama

```csharp
var emailMessage = new EmailMessageBuilder()
    .AddFrom("noreply@mydomain.com")
    .AddRecipient("John Doe", "johndoe@example.com", RecipientType.To)
    .AddRecipient("Support Team", "support@mydomain.com", RecipientType.Cc)
    .AddSubject("Hoş Geldiniz!")
    .AddBody("<p>Merhaba John, hesabınız başarıyla oluşturuldu.</p>", isHtml: true)
    .MarkAsImportant()
    .Build();
```

### 3.2 E-posta Gönderimi

```csharp
public class NotificationService
{
    private readonly IMailService _mailService;

    public NotificationService(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task SendWelcomeEmailAsync(string recipientEmail, string recipientName)
    {
        var emailMessage = new EmailMessageBuilder()
            .AddFrom("noreply@mydomain.com")
            .AddRecipient(recipientName, recipientEmail, RecipientType.To)
            .AddSubject("Hoş Geldiniz!")
            .AddBody("<p>Merhaba, hesabınız başarıyla oluşturuldu.</p>", isHtml: true)
            .Build();

        await _mailService.SendEmailAsync(emailMessage);
    }
}
```

### 3.3 Behavior Kullanımı

Behavior sınıfları MediatR pipeline'ında otomatik olarak çalışır. Aşağıda **EmailLoggingBehavior** ve **EmailSendingBehavior** örneklerini görebilirsiniz:

#### EmailLoggingBehavior

```csharp
public class EmailLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<EmailLoggingBehavior<TRequest, TResponse>> _logger;

    public EmailLoggingBehavior(ILogger<EmailLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request: {Request}", request);
        var response = await next();
        _logger.LogInformation("Handled request: {Response}", response);
        return response;
    }
}
```

#### EmailSendingBehavior

```csharp
public class EmailSendingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IMailService _mailService;

    public EmailSendingBehavior(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is EmailMessage emailMessage)
        {
            await _mailService.SendEmailAsync(emailMessage);
        }

        return response;
    }
}
```

---

## 4. Sağlayıcıların Çalışma Sırası

- **EmailSendingService**, tanımlanan sağlayıcıları sırayla dener. Eğer bir sağlayıcı hata verirse bir sonrakine geçer.
- Sağlayıcılar başarısız olursa, `InvalidOperationException` fırlatılır.

---

## 5. Yeni Sağlayıcı Ekleme

Yeni bir sağlayıcı eklemek için aşağıdaki adımları izleyin:

1. `IEmailProvider` arayüzünü implement edin.
2. Sağlayıcıyı DI container'a ekleyin.
3. Örnek:

```csharp
public class AmazonSesEmailProvider : IEmailProvider
{
    private readonly ILogger<AmazonSesEmailProvider> _logger;

    public AmazonSesEmailProvider(ILogger<AmazonSesEmailProvider> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage emailMessage)
    {
        // Amazon SES API entegrasyonu
        _logger.LogInformation("Amazon SES üzerinden e-posta gönderiliyor...");
        return Task.CompletedTask;
    }
}
```

---