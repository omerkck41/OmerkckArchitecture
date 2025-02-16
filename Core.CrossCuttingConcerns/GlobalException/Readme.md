# 📘 Global Exception Handling Kütüphanesi

## 📝 **Nedir?**
Bu kütüphane, ASP.NET Core projelerinde istisnaları merkezi bir noktada ele almak için geliştirilmiştir. `GlobalExceptionHandler`, `ValidationExceptionHandler` ve `ExceptionHandlerFactory` gibi bileşenlerle, hata yönetimini standartlaştırarak API'nin daha güvenilir ve tutarlı olmasını sağlar.

---
## 💡 **Neden Kullanılır?**
- **Merkezi Hata Yönetimi:** Tüm istisnaları tek bir noktadan yakalayarak yönetir.
- **API Standartlarına Uyum:** `ProblemDetails` formatı sayesinde tutarlı hata yanıtları sağlar.
- **Modülerlik:** Farklı hata türleri için özel handler’lar oluşturulabilir.
- **Kolay Entegrasyon:** Projeye birkaç satır kod ekleyerek kullanılabilir.

---
## 🚀 **Avantajları:**
- ✅ **Performans:** `WriteAsJsonAsync()` ile hızlı yanıtlar.
- ✅ **Standartlaştırma:** `ProblemDetails` kullanımı ile API'lerde ortak hata formatı.
- ✅ **Genişletilebilirlik:** Yeni hata türleri için kolayca ek handler’lar oluşturma.
- ✅ **Bakım Kolaylığı:** Hata yönetimi merkezi bir yapıda tutulur.

---
## 🛠️ **Projeye Nasıl Eklenir?**
### 📂 **1. Bağımlılıkların Yüklenmesi:**
Gerekli bağımlılıkları `Program.cs` içinde ekleyin:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionMiddlewareServices();

var app = builder.Build();
app.UseExceptionMiddleware();
app.Run();
```

### 📝 **2. appsettings.json Ayarları:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---
## 📊 **Detaylı Kullanım Örnekleri:**
### 🎯 **Validation Hatası Örneği:**
API'ye geçersiz bir e-posta gönderildiğinde:
```json
{
  "status": 400,
  "title": "Validation error",
  "detail": "Validation failed for one or more fields.",
  "errors": {
    "Email": ["Geçerli bir e-posta adresi değil."]
  }
}
```
### 🎯 **Genel Sistem Hatası Örneği:**
```json
{
  "status": 500,
  "title": "An unexpected error occurred",
  "detail": "Null reference exception",
  "instance": "2d5f9f10-7634-42d4-bd8a-8c9f53f6e788"
}
```
---
## 🏁 **Sonuç:**
Bu yapı sayesinde projelerinizde merkezi hata yönetimi sağlayarak, API'lerinizi daha güvenilir, tutarlı ve bakımı kolay hale getirebilirsiniz. 🚀

