# Core.Toolkit Session Management Module

**Session Management Module**, ASP.NET Core projelerinde kullanıcı oturumu ve çerez yönetimi için güçlü araçlar sunar. Bu araçlar, yüksek performanslı ve modüler bir yapıyla oturum verilerini saklama, okuma ve temizleme işlemlerini kolaylaştırır.

---

## **1. Session İşlemleri (SessionHelper)**

### **Açıklama**
Kullanıcı oturumunda verileri saklamak, okumak ve yönetmek için kullanılır.

### **Kullanım**

#### **Oturumda Veri Saklama**
```csharp
using Core.Toolkit.SessionManagement;
using Microsoft.AspNetCore.Http;

// Bir nesneyi oturuma kaydetme
HttpContext.Session.Set("User", new { Name = "John", Age = 30 });
```

#### **Oturumdan Veri Okuma**
```csharp
using Core.Toolkit.SessionManagement;
using Microsoft.AspNetCore.Http;

// Oturumdan bir nesneyi okuma
var user = HttpContext.Session.Get<dynamic>("User");
Console.WriteLine(user?.Name);
```

#### **Oturumdan Veri Silme**
```csharp
using Core.Toolkit.SessionManagement;
using Microsoft.AspNetCore.Http;

// Belirli bir anahtarı oturumdan kaldırma
HttpContext.Session.Remove("User");
```

#### **Tüm Oturum Verilerini Temizleme**
```csharp
using Core.Toolkit.SessionManagement;
using Microsoft.AspNetCore.Http;

// Tüm oturum verilerini temizleme
HttpContext.Session.Clear();
```

---

## **2. Cookie İşlemleri (CookieHelper)**

### **Açıklama**
Çerezleri yönetmek için kullanılır. Çerezleri güvenli ve özelleştirilebilir şekilde saklama, okuma ve silme işlemleri sağlar.

### **Kullanım**

#### **Çerez Oluşturma**
```csharp
using Core.Toolkit.SessionManagement;
using Microsoft.AspNetCore.Http;

// Bir çerez oluşturma
CookieHelper.Set(HttpContext.Response, "AuthToken", "abcdef12345", new CookieOptions
{
    Expires = DateTime.UtcNow.AddHours(1),
    HttpOnly = true,
    Secure = true
});
```

#### **Çerez Okuma**
```csharp
using Core.Toolkit.SessionManagement;
using Microsoft.AspNetCore.Http;

// Bir çerezi okuma
var token = CookieHelper.Get(HttpContext.Request, "AuthToken");
Console.WriteLine(token);
```

#### **Çerez Silme**
```csharp
using Core.Toolkit.SessionManagement;
using Microsoft.AspNetCore.Http;

// Bir çerezi silme
CookieHelper.Remove(HttpContext.Response, "AuthToken");
```

---

## **Özellikler ve Avantajlar**

- **Yüksek Performans**: Oturum ve çerez işlemleri hızlı ve optimize edilmiştir.
- **Kolay Entegrasyon**: ASP.NET Core projelerinde kolayca kullanılabilir.
- **Güvenli Çerezler**: HttpOnly ve Secure seçenekleriyle çerez güvenliği sağlar.
- **Hata Yönetimi**: Eksik veya hatalı parametrelerde detaylı hata mesajları sağlar.

---

## **Pratik Örnekler**

### **Örnek 1: Kullanıcı Giriş Verilerini Saklama**
```csharp
HttpContext.Session.Set("LoggedInUser", new { UserId = 123, Role = "Admin" });
```

### **Örnek 2: Çerez ile Kimlik Doğrulama**
```csharp
// Çerez oluşturma
CookieHelper.Set(HttpContext.Response, "SessionId", "xyz-123", new CookieOptions
{
    Expires = DateTime.UtcNow.AddDays(1),
    HttpOnly = true
});

// Çerezi okuma
var sessionId = CookieHelper.Get(HttpContext.Request, "SessionId");
```

---
