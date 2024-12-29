# Core.Application.Mailing Kullanım Kılavuzu

## Genel Bakış
`Core.Application.Mailing` modülü, uygulamanız için sağlam, genişletilebilir ve ölçeklenebilir bir e-posta gönderim çerçevesi sunar. Hem **Davranış Tabanlı (Behavior-based)** hem de **Servis Tabanlı (Service-based)** kullanım için destek sağlar, böylece geliştiriciler ihtiyaçlarına en uygun entegrasyon yöntemini seçebilirler.

### Temel Özellikler
- **Davranış Tabanlı Entegrasyon**: MediatR pipeline desteği ile otomatik e-posta işlemleri.
- **Servis Tabanlı Entegrasyon**: Manuel e-posta işlemleri için IMailService.
- **SMTP İstemci Yönetimi**: Performansı artırmak için hız sınırlamalı SMTP istemci seçimi.
- **E-posta Oluşturucu**: `EmailMessageBuilder` ile basitleştirilmiş e-posta oluşturma.
- **Loglama ve Hata Yönetimi**: ILogger ile yerleşik loglama desteği.
- **Öncelik Desteği**: E-postaları önemli olarak işaretleme.

### Avantajlar
- **Esneklik**: Hem Davranış hem de Servis kullanımı ile sorunsuz çalışır.
- **Ölçeklenebilirlik**: Hız sınırlamalı birden fazla SMTP istemcisini destekler.
- **Özelleştirilebilirlik**: Çeşitli proje gereksinimleri için genişletilebilir mimari.
- **En İyi Uygulamalar**: Clean code prensipleri ve sorumlulukların ayrılması.

---

## Entegrasyon

### Mailing Modülünü Projeye Ekleme
1. **Referans Ekleyin**: Projenizde `Core.Application.Mailing` modülünü referans olarak ekleyin.
2. **Gerekli Bağımlılıkları Yükleyin**:
   - Davranış tabanlı kullanım için MediatR ekleyin.
   - Loglama için bir kütüphane (örn. Serilog, NLog) ekleyin.

3. **Konfigürasyon Ayarları**:
   `appsettings.json` dosyanıza SMTP ayarlarını ekleyin:
   ```json
   {
     "SmtpSettings": {
       "Host": "smtp.example.com",
       "Port": 587,
       "Username": "your_username",
       "Password": "your_password",
       "EnableSsl": true
     }
   }
   ```

4. **Bağımlılıkları DI Konteynerine Kaydedin**:
   `Program.cs` veya `Startup.cs` dosyanıza şu kodları ekleyin:
   ```csharp
   services.AddTransient<IMailService, SmtpMailService>();
   services.AddSingleton<ISmtpClientSelector, RateLimitingSmtpClientSelector>();
   services.AddTransient<IPipelineBehavior<,>, EmailSendingBehavior<,>>();
   services.AddTransient<IPipelineBehavior<,>, EmailLoggingBehavior<,>>();

   services.Configure<SmtpClientOptions>(configuration.GetSection("SmtpSettings"));
   services.AddSingleton<SmtpClient>(sp => {
       var options = sp.GetRequiredService<IOptions<SmtpClientOptions>>().Value;
       return new SmtpClient
       {
           Host = options.Host,
           Port = options.Port,
           Credentials = new NetworkCredential(options.Username, options.Password),
           EnableSsl = options.EnableSsl
       };
   });
   ```

---

## Kullanım

### Davranış Tabanlı Kullanım
Davranış tabanlı entegrasyon, MediatR pipeline sırasında e-posta gönderimini otomatik olarak işler.

1. **MediatR Komutu Oluşturun**:
   ```csharp
   public class SendNotificationCommand : IRequest
   {
       public string Recipient { get; set; }
       public string Subject { get; set; }
       public string Message { get; set; }
   }
   ```

2. **Komutu Bir Handler'da Kullanın**:
   ```csharp
   public class SendNotificationHandler : IRequestHandler<SendNotificationCommand>
   {
       public async Task<Unit> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
       {
           var emailMessage = new EmailMessageBuilder()
               .AddFrom("noreply@example.com")
               .AddTo(request.Recipient)
               .AddSubject(request.Subject)
               .AddBody(request.Message)
               .Build();

           // EmailSendingBehavior bunu otomatik olarak işleyecektir
           return Unit.Value;
       }
   }
   ```

### Servis Tabanlı Kullanım
Manuel kontrol için IMailService doğrudan kullanılabilir.

1. **IMailService'i Enjekte Edin**:
   ```csharp
   public class EmailController : ControllerBase
   {
       private readonly IMailService _mailService;

       public EmailController(IMailService mailService)
       {
           _mailService = mailService;
       }

       [HttpPost("send-email")]
       public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
       {
           var emailMessage = new EmailMessageBuilder()
               .AddFrom("noreply@example.com")
               .AddTo(emailRequest.To)
               .AddSubject(emailRequest.Subject)
               .AddBody(emailRequest.Body)
               .MarkAsImportant()
               .Build();

           await _mailService.SendEmailAsync(emailMessage);
           return Ok("Email başarıyla gönderildi.");
       }
   }
   ```

2. **EmailRequest DTO**:
   ```csharp
   public class EmailRequest
   {
       public string To { get; set; }
       public string Subject { get; set; }
       public string Body { get; set; }
   }
   ```

---

## Gelişmiş Özellikler

### SMTP Hız Sınırlandırma
Birden fazla SMTP istemcisini verimli bir şekilde yönetmek için `RateLimitingSmtpClientSelector` kullanılır. Bu, yüksek hacimli e-posta gönderen uygulamalar için idealdir.

1. **Birden Fazla SMTP İstemcisi Yapılandırın**:
   ```csharp
   services.AddSingleton<ISmtpClientSelector>(sp => new RateLimitingSmtpClientSelector(new List<SmtpClient>
   {
       new SmtpClient("smtp1.example.com", 587),
       new SmtpClient("smtp2.example.com", 587)
   }, maxSendsPerClient: 100));
   ```

2. **Özelleştirilmiş Mantık**:
   Ek seçim kriterleri gerekiyorsa `ISmtpClientSelector` genişletilebilir.

---

## Özet
`Core.Application.Mailing` modülü, temiz ve ölçeklenebilir bir şekilde e-posta işlemlerini yönetmek için esnek ve güçlü bir çerçeve sunar. Davranış tabanlı otomasyon veya servis tabanlı manuel kontrol tercihinize bağlı olarak bu kütüphane ihtiyaçlarınızı karşılamak için tasarlanmıştır.