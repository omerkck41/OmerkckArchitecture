# Core.Notifications Modülü - Entegrasyon ve Kullanım Rehberi

## **Giriş**
Core.Notifications modülü, büyük projelerde kullanıcı bildirimlerini yönetmek için tasarlanmıştır. E-posta, SMS, push notification ve gerçek zamanlı SignalR tabanlı bildirim desteği sağlar. Bu modül, genişletilebilir ve temiz kod prensiplerine göre hazırlanmıştır.

---

## **1. Projeye Entegrasyon**

### **Adım 1: NuGet Paketlerinin Kurulumu**
Core.Notifications modülünün sorunsuz çalışması için aşağıdaki paketlerin projeye dahil edilmesi gereklidir:

```bash
Install-Package Microsoft.AspNetCore.SignalR
Install-Package MailKit
Install-Package Twilio
Install-Package RazorLight
```

### **Paketlerin Görevleri:**
- **Microsoft.AspNetCore.SignalR**: Gerçek zamanlı bildirimler için SignalR altyapısı sağlar.
- **MailKit**: E-posta gönderimi için SMTP istemcisi.
- **Twilio**: SMS gönderimi için üçüncü taraf Twilio servisi.
- **RazorLight**: HTML şablonlarını kullanarak dinamik e-posta içerikleri oluşturur.

---

### **Adım 2: Proje Referansı Ekleme**
Core.Notifications modülünü mevcut projeye submodule olarak eklemek için aşağıdaki komut kullanılabilir:

```bash
git submodule add <Core.Notifications Git Repository URL> Core.Notifications
```

CLI üzerinden Core.Notifications projesine referans ekleyin:
```bash
dotnet add reference Core.Notifications/Core.Notifications.csproj
```

---

### **Adım 3: Program.cs Yapılandırması**
Core.Notifications modülündeki servisleri kullanabilmek için `Program.cs` dosyasına aşağıdaki servis kayıtlarını ekleyin:

```csharp
using Core.Notifications.EmailNotifications;
using Core.Notifications.SmsNotifications;
using Core.Notifications.PushNotifications;
using Core.Notifications.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Servis Kayıtları
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ISmsService, SmsService>();
builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

// SignalR Ayarları
builder.Services.AddSignalR();

var app = builder.Build();

// SignalR Hub Yolu
app.MapHub<NotificationHub>("/notifications");

app.Run();
```

---

### **Adım 4: appsettings.json Yapılandırması**
E-posta ve SMS gönderimleri için yapılandırmaları `appsettings.json` dosyasına ekleyin:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.example.com",
    "Port": 587,
    "Username": "your-email@example.com",
    "Password": "your-email-password",
    "From": "no-reply@example.com"
  },
  "SmsSettings": {
    "TwilioAccountSid": "YourTwilioAccountSid",
    "TwilioAuthToken": "YourTwilioAuthToken",
    "FromPhoneNumber": "+123456789"
  }
}
```

---

## **2. Kullanım Amaçları ve Avantajları**
### **Kullanım Amaçları:**
- Kullanıcılara e-posta, SMS ve push notification yoluyla bildirim gönderme.
- Gerçek zamanlı bildirimler ile kullanıcı etkileşimini artırma.
- Dinamik bildirim şablonları sayesinde özelleştirilmiş içerikler sunma.

### **Avantajları:**
- **Modüler Yapı**: Sadece ihtiyacınız olan servisleri kullanabilirsiniz.
- **Kolay Entegrasyon**: Program.cs ve appsettings.json yapılandırması ile hızlı kurulum.
- **Gerçek Zamanlı Bildirimler**: SignalR ile anlık iletişim sağlanır.
- **Genişletilebilirlik**: İhtiyaca göre e-posta, SMS veya push servis sağlayıcıları değiştirilebilir.

---

## **3. Kullanım Örnekleri**

### **3.1. E-Posta Gönderimi**
```csharp
var emailService = app.Services.GetRequiredService<IEmailService>();
await emailService.SendEmailAsync(
    to: "user@example.com",
    subject: "Welcome to Our Service",
    body: "<h1>Welcome!</h1><p>Thank you for signing up.</p>"
);
```

---

### **3.2. SMS Gönderimi**
```csharp
var smsService = app.Services.GetRequiredService<ISmsService>();
await smsService.SendSmsAsync(
    phoneNumber: "+905555555555",
    message: "Your verification code is 123456"
);
```

---

### **3.3. Push Notification Gönderimi**
```csharp
var pushService = app.Services.GetRequiredService<IPushNotificationService>();
await pushService.SendPushNotificationAsync(
    deviceToken: "DeviceToken123",
    title: "New Alert",
    message: "You have a new notification."
);
```

---

### **3.4. Gerçek Zamanlı Bildirim Gönderimi (SignalR)**
#### **Server-Side Kullanım:**
```csharp
var notificationService = app.Services.GetRequiredService<INotificationService>();
await notificationService.SendRealTimeNotificationAsync("This is a real-time notification.");
```

#### **Client-Side Kullanım (JavaScript):**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notifications")
    .build();

connection.on("ReceiveNotification", function (message) {
    console.log("Notification received: " + message);
});

connection.start().then(function () {
    console.log("Connected to Notification Hub.");
}).catch(function (err) {
    console.error(err.toString());
});
```

---

## **Sonuç**
Core.Notifications modülü, projelerde kullanıcı bildirimlerini e-posta, SMS, push notification ve SignalR tabanlı gerçek zamanlı bildirimlerle yönetmek için güçlü bir çözüm sunar. Modüler ve genişletilebilir yapısıyla büyük ölçekli projelerde ihtiyaçları karşılayacak şekilde tasarlanmıştır. Projenize hızlı bir şekilde entegre ederek kullanıcı etkileşimini artırabilirsiniz.
