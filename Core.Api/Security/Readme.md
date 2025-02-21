# API Güvenlik Kütüphanesi Kullanım Kılavuzu

## 📌 **Proje Hakkında**
Bu kütüphane, **ASP.NET Core API** projelerinde güvenliği sağlamak için geliştirilmiştir. **Middleware tabanlı bir yapı** kullanılarak şu güvenlik mekanizmaları sağlanır:

- **CORS Yönetimi** → Belirtilen domainlerden gelen istekleri kabul eder.
- **IP Whitelist** → Belirtilen IP adreslerinden erişime izin verir.
- **Rate Limiting** → Kötüye kullanımı önlemek için istekleri sınırlar.
- **HTTPS Zorunluluğu** → API’nin sadece HTTPS üzerinden çalışmasını sağlar.
- **Brute Force Koruması** → Tekrarlayan başarısız giriş denemelerini sınırlar.
- **CSRF Koruması** → Cross-Site Request Forgery saldırılarını engeller.
- **Gelişmiş Güvenlik Başlıkları** → XSS, Clickjacking gibi saldırılara karşı ekstra koruma sağlar.

---

## 🔧 **Kurulum ve Kullanım**

### **1️⃣ SecuritySettings Yapılandırması**
📌 **Projenin `appsettings.json` dosyasına şu ayarları ekleyin:**

```json
{
  "SecuritySettings": {
    "AllowedIPs": ["127.0.0.1", "::1"],
    "AddCorsPolicy": ["https://localhost:3000"],
    "RateLimit": 100,
    "MaxLoginAttempts": 5,
    "LockoutTime": 5,
    "EnforceHttps": false,
    "EnableCsrfProtection": true,
    "CsrfExcludedEndpoints": ["/api/Auth/Login", "/api/Auth/Register"]
  }
}
```

---

### **2️⃣ Security Middleware ve Servislerini Projeye Entegre Etme**
📌 **`Program.cs` içinde `SecurityExtensions` ve `CorsManager` çağırılmalıdır.**

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Security Middleware'leri ekle
builder.Services.AddSecurityServices(builder.Configuration);

// CORS ayarlarını uygula
app.UseCors("DefaultCorsPolicy");

// Middleware’leri uygula
app.UseSecurityMiddlewares(builder.Configuration);

app.Run();
```

---

## 🚀 **Middleware Açıklamaları ve Kullanımı**

### **1️⃣ IP Whitelist Middleware (`IpWhitelistMiddleware`)**
📌 **Sadece belirlenen IP’lerin API’ye erişmesine izin verir.**

**💡 Kullanım:** `SecuritySettings.AllowedIPs` listesinde belirtilen IP adresleri haricindeki istekler engellenir.

---

### **2️⃣ Rate Limiting Middleware (`RateLimiterMiddleware`)**
📌 **Belirli bir zaman diliminde yapılan istek sayısını sınırlandırır.**

**💡 Kullanım:** `SecuritySettings.RateLimit` değeri kullanılarak her IP için saniyelik istek limiti belirlenir.

---

### **3️⃣ HTTPS Zorunluluğu (`HttpsEnforcerMiddleware`)**
📌 **Sadece HTTPS bağlantılarını kabul eder.**

**💡 Kullanım:** `SecuritySettings.EnforceHttps` değeri `true` ise API sadece HTTPS üzerinden çalışır.

---

### **4️⃣ Brute Force Koruması (`BruteForceProtectionMiddleware`)**
📌 **Belirli sayıda başarısız giriş denemesinden sonra kullanıcıyı bloke eder.**

**💡 Kullanım:** `SecuritySettings.MaxLoginAttempts` ve `SecuritySettings.LockoutTime` kullanılarak yapılandırılır.

---

### **5️⃣ CSRF Koruması (`AntiForgeryMiddleware`)**
📌 **CSRF saldırılarını engellemek için isteklerde `X-CSRF-TOKEN` başlığını doğrular.**

**💡 Kullanım:**
- `SecuritySettings.EnableCsrfProtection = true` olmalı.
- CSRF’den muaf tutulacak endpointler `SecuritySettings.CsrfExcludedEndpoints` listesine eklenmelidir.

**💡 Login İşlemi Sonrası CSRF Token Üretme ve Kullanma:**

```csharp
private void setRefreshTokenToCookie(RefreshToken refreshToken)
{
    Response.Cookies.Append("X-CSRF-TOKEN", Guid.NewGuid().ToString(), new CookieOptions
    {
        HttpOnly = false,
        SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(7)
    });
}
```

**Frontend CSRF Token Gönderme Örneği (JavaScript - Fetch API)**
```javascript
const csrfToken = document.cookie.split('; ').find(row => row.startsWith('X-CSRF-TOKEN'))?.split('=')[1];
fetch("https://localhost:5001/api/user/profile", {
    method: "GET",
    headers: {
        "Authorization": "Bearer " + localStorage.getItem("accessToken"),
        "X-CSRF-TOKEN": csrfToken
    }
})
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error("Hata:", error));
```

---

### **6️⃣ Gelişmiş Güvenlik Başlıkları (`SecurityHeadersMiddleware`)**
📌 **API yanıtlarına aşağıdaki güvenlik başlıklarını ekler:**
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `X-Content-Type-Options: nosniff`
- `Strict-Transport-Security: max-age=31536000; includeSubDomains` (Sadece HTTPS için)

---

## 🔗 **Sonuç ve Özet**
Bu kütüphane sayesinde **ASP.NET Core API projelerinde güvenliği artırabilir**, **yetkisiz erişimi engelleyebilir** ve **daha sağlam bir altyapı oluşturabilirsin**.
