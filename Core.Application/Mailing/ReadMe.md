Tabii, son güncellemelerle birlikte `ReadMe.md` dosyanızı güncelleyelim. Özellikle `IEmailRequest` arayüzü ve `EmailSendingBehavior`'ın genişletilmiş kullanımına odaklanarak, README dosyasını daha detaylı ve kullanıcı dostu hale getireceğim.

---

# Mail Gönderme Kütüphanesi

Mailing Service, uygulamanızdan e-posta gönderimi yapabilmenizi sağlayan, modüler ve çoklu e-posta sağlayıcı desteği sunan bir altyapıdır. Proje, SMTP, SendGrid ve Amazon SES gibi farklı e-posta gönderim yöntemlerini destekler. Merkezi konfigürasyon yönetimi sayesinde, e-posta gönderiminde kullanılan varsayılan bilgiler (örn. From ve FromName) tek bir noktadan ayarlanır.

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

## Neden Kullanılır?
- **Merkezi Konfigürasyon: Tüm e-posta gönderim ayarları EmailSettings adlı strongly-typed model üzerinden yönetilir.
- **Çoklu Sağlayıcı Desteği: SMTP, SendGrid ve Amazon SES gibi farklı e-posta sağlayıcıları desteklenir; böylece ihtiyaç duyduğunuzda sağlayıcılar arasında geçiş yapabilirsiniz.
- **Varsayılan Değerlerin Uygulanması: EmailSendingService içerisinde yer alan ApplyDefaults metodu ile, e-posta mesajları oluşturulurken otomatik olarak varsayılan From ve FromName değerleri eklenir.
- **Modüler Yapı: Yeni e-posta sağlayıcılarını sisteme eklemek ve mevcut olanları güncellemek oldukça kolaydır.
- **Test Edilebilirlik ve Bakım Kolaylığı: Bağımsız servis ve sağlayıcıların kullanılması, birim testler ve ileride yapılacak bakım işlemlerini basitleştirir.

## Avantajları
- **Tek Noktadan Ayar Yönetimi: EmailSettings sayesinde tüm e-posta gönderim ayarları merkezi olarak konfigüre edilir.
- **Azaltılmış Kod Tekrarı: Default değer atamaları, ApplyDefaults metodu ile merkezi olarak uygulanır.
- **Genişletilebilirlik: Yeni e-posta sağlayıcılarını eklemek veya mevcut sağlayıcıları değiştirmek kolaydır.
- **Hata Yönetimi & Logging: Uygulamada meydana gelen hatalar merkezi olarak ele alınır, ayrıca loglama mekanizmaları ile desteklenir.

---
## Kurulum

### 1. Projeye Nasıl Eklenir?

1. **NuGet Paketleri:** Projede aşağıdaki NuGet paketlerinin yüklü olduğundan emin olun:
- **Microsoft.Extensions.DependencyInjection
- **Microsoft.Extensions.Options
- **(Amazon SES için) AWSSDK.SimpleEmailV2
- **Diğer sağlayıcılar için ilgili paketler (örn. SendGrid)

---

### 2. Ayar Dosyalarının Düzenlenmesi

`appsettings.json`
Projenizin kök dizininde bulunan appsettings.json dosyanıza aşağıdaki örnek konfigürasyonu ekleyin:

#### `appsettings.json` Örneği:

```json
{
  "EmailSettings": {
    "DefaultFromAddress": "noreply@ornek.com",
    "DefaultFromName": "Destek Ekibi",
    "SendGridApiKey": "your-sendgrid-api-key",
    "AwsRegion": "us-east-1",
    "AwsAccessKey": "YOUR_AWS_ACCESS_KEY",
    "AwsSecretKey": "YOUR_AWS_SECRET_KEY",
    "PreferredProvider": "Smtp",
    "MaxEmailsPerClient": 100,
    "SmtpServers": [
      {
        "Host": "mail.ornek.com",
        "Port": 587,
        "UseSsl": true,
        "Username": "test@ornek.com",
        "Password": "sifre123"
      }
    ],
  }
}
```

---

### 3. DI Container’a Servislerin Eklenmesi

Mailing servisi ve ilgili bağımlılıkları DI container’a eklemek için ServiceCollectionExtensions sınıfını kullanabilirsiniz.

Program.cs (.NET 6 ve sonrası) Örneği:
```csharp
var builder = WebApplication.CreateBuilder(args);

// appsettings.json konfigürasyonunu ekleyin
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Mailing servislerini IOptions pattern ile register edin
builder.Services.AddMailingServicesFromJson(builder.Configuration);

var app = builder.Build();

// Diğer middleware ve yapılandırmalar

app.Run();

```
---

Startup.cs (.NET Core 3.1 veya .NET 5) Örneği:
```csharp
public class Startup
{
    public IConfiguration Configuration { get; }
    
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMailingServicesFromJson(Configuration);
        // Diğer servis kayıtları...
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Middleware yapılandırmaları...
    }
}
```
---

## Kullanım

### 1. Mail Mesajı Oluşturma

- **Örnek 1: Basit E-posta Gönderimi
Aşağıdaki örnekte, kullanıcı doğrulama e-postası göndermek için EmailSendingService kullanılmıştır:

```csharp
using Core.Application.Mailing.Models;
using Core.Application.Mailing.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AccountService
{
    private readonly IMailService _mailService;

    public AccountService(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task SendVerificationEmailAsync(User user, string verificationLink)
    {
        var emailMessage = new EmailMessage
        {
            Recipients = new List<EmailRecipient>
            {
                new EmailRecipient 
                { 
                    Name = $"{user.FirstName} {user.LastName}", 
                    Email = user.Email, 
                    Type = RecipientType.To 
                }
            },
            Subject = "E-posta Doğrulama",
            Body = $"E-posta adresinizi doğrulamak için lütfen aşağıdaki linke tıklayın: {verificationLink}",
            IsHtml = false
        };

        // EmailSendingService içerisinde default değerler (From, FromName) otomatik olarak uygulanır
        await _mailService.SendEmailAsync(emailMessage);
    }
}

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

### Genel Mimari
**EmailSettings:**
Uygulamanın e-posta gönderim ayarlarını (varsayılan gönderici, AWS bilgileri, SMTP sunucuları vb.) tutan strongly-typed konfigürasyon modelidir.

**EmailMessage:**
E-posta içeriği, alıcı bilgileri, konu ve gövde gibi bilgileri barındıran DTO (Data Transfer Object) sınıfıdır.

**EmailSendingService:**
E-posta gönderim işlemlerini yöneten servis sınıfıdır. Bu sınıf, ApplyDefaults metodu aracılığıyla tüm mesajlarda varsayılan değerlerin uygulanmasını sağlar ve kayıtlı tüm e-posta sağlayıcılarını kullanarak gönderimi gerçekleştirir.

**IEmailProvider Implementasyonları:**
Farklı e-posta gönderim yöntemlerini destekleyen sağlayıcı sınıfları:

**SmtpEmailProvider:** SMTP üzerinden e-posta gönderimi gerçekleştirir.
**SendGridEmailProvider:** SendGrid üzerinden e-posta gönderimi gerçekleştirir.
**AmazonSesEmailProvider:** Amazon SES üzerinden e-posta gönderimi gerçekleştirir.
**ISmtpClientSelector (Opsiyonel):**
SMTP istemcileri arasında rate-limiting uygulanmasını sağlayan yardımcı sınıftır.

---