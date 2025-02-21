# Core.Api.Security - API Güvenlik Katmanı

## 📌 **Genel Bakış**
**Core.Api.Security**, .NET tabanlı API projelerinde güvenliği artırmak amacıyla geliştirilmiş bir modüldür. 
Bu kütüphane, API’nizin saldırılara karşı korunmasına yardımcı olmak için çeşitli **Middleware’ler** ve **Servisler** sağlar. 
Özellikle büyük ölçekli projelerde güvenliği sağlamak için kritik öneme sahiptir.

---

## 🎯 **Neden Core.Api.Security Kullanılmalı?**

Güvenli bir API geliştirmek için **Core.Api.Security** kullanmanın başlıca avantajları:

✅ **IP Whitelist**: API’ye yalnızca belirli IP’lerin erişmesine izin verir. 
✅ **Rate Limiting**: API isteklerini sınırlayarak **DoS ve Brute-Force saldırılarını** önler. 
✅ **HTTPS Enforcement**: API’nin yalnızca **HTTPS üzerinden çalışmasını zorunlu kılar**.
✅ **Gelişmiş Güvenlik Başlıkları**: **XSS, Clickjacking, CSRF gibi saldırılara** karşı ekstra güvenlik sağlar.
✅ **Request Validation**: Zararlı SQL Injection veya kötü niyetli istekleri otomatik olarak engeller.
✅ **CORS Yönetimi**: API'ye sadece **belirlenen domainlerden** erişimi sağlar.

### ⚠ **Olası Dezavantajlar**
- Tüm güvenlik önlemleri **performans açısından bir miktar yük getirebilir**.
- Yanlış yapılandırılırsa **meşru istekleri de engelleyebilir**.
- Rate Limiting gibi mekanizmalar, çok fazla eş zamanlı isteğe sahip projelerde **dikkatle ayarlanmalıdır**.

---

## 🛠 **Projeye Entegrasyon**

### **1️⃣. Manuel Kurulum (Kütüphaneyi Projeye Dahil Etme)**
Eğer NuGet üzerinden yüklemek yerine doğrudan projeye entegre etmek istiyorsanız, **Core.Api.Security klasörünü** projenize dahil edin:

📂 **Proje Yapısı:**
```
Core.Api/
│── Security/
│   │── Middleware/  
│   │   ├── IpWhitelistMiddleware.cs
│   │   ├── RateLimiterMiddleware.cs
│   │   ├── HttpsEnforcerMiddleware.cs
│   │   ├── SecurityHeadersMiddleware.cs
│   │   ├── RequestValidationMiddleware.cs
│   │   ├── AntiForgeryMiddleware.cs
│   │   ├── BruteForceProtectionMiddleware.cs
│   │── Services/
│   │   ├── CorsManager.cs
│   │── Extensions/
│   │   ├── SecurityExtensions.cs
```

### **3️⃣. `appsettings.json` Üzerinden Yapılandırma**

📌 **Güvenlik ayarları için** `appsettings.json` içinde aşağıdaki bölümü ekleyin:
```json
{
  "SecuritySettings": {
    "AllowedIPs": [
      "192.168.1.100",
      "203.0.113.42"
    ],
    "AddCorsPolicy": [
      "https://yourfrontend.com",
      "https://yourfrontend2.com"
    ],
    "RateLimit": 100
  }
}
```

### **4️⃣. `Program.cs` İçinde Middleware ve Güvenlik Yapılandırmasını Aktif Etme**

📌 **`Program.cs` dosyanıza aşağıdaki kodları ekleyin:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Güvenlik ayarlarını servis olarak ekleyin
builder.Services.AddSecurityServices(builder.Configuration);

var app = builder.Build();

// Middleware’leri API’ye entegre et
app.UseSecurityMiddlewares(builder.Configuration);

app.MapControllers();
app.Run();
```

---

## 🚀 **Middleware Kullanımı ve Detayları**

### **IP Whitelist Kullanımı (IpWhitelistMiddleware)**
Belirtilen IP adresleri dışındaki tüm erişimleri engeller.
```csharp
app.UseMiddleware<IpWhitelistMiddleware>();
```

### **Rate Limiting Kullanımı (RateLimiterMiddleware)**
Aşırı istek atan kullanıcıları engeller.
```csharp
app.UseMiddleware<RateLimiterMiddleware>();
```

### **HTTPS Zorunluluğu (HttpsEnforcerMiddleware)**
API’ye yalnızca **HTTPS üzerinden erişime** izin verir.
```csharp
app.UseMiddleware<HttpsEnforcerMiddleware>();
```

### **Gelişmiş Güvenlik Başlıkları (SecurityHeadersMiddleware)**
Clickjacking, XSS, CSRF gibi saldırılara karşı ek başlıklar ekler.
```csharp
app.UseMiddleware<SecurityHeadersMiddleware>();
```

---

## 🎯 **Sonuç**
✅ **Core.Api.Security**, API güvenliğini artırmak için modüler ve genişletilebilir bir çözümdür. 
Bu kütüphane, saldırılara karşı ek bir katman oluştururken, API’nizin daha güvenli ve ölçeklenebilir olmasını sağlar.**

📌 **Özetle:**
- 🚀 **IP Whitelist, Rate Limiting, HTTPS Zorunluluğu gibi mekanizmalar içerir.**
- 🔐 **XSS, Clickjacking, CSRF saldırılarına karşı ek güvenlik başlıkları sağlar.**
- 🛠 **Kolay entegrasyon ve esnek yapılandırma seçenekleri sunar.**
