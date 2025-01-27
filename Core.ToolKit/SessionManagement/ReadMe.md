# CookieHelper ve SessionHelper Kütüphaneleri

Bu kütüphaneler, ASP.NET Core projelerinde cookie ve session yönetimini kolaylaştırmak için geliştirilmiştir. `CookieHelper` ve `SessionHelper` sınıfları, cookie ve session işlemlerini daha temiz, güvenli ve asenkron bir şekilde yönetmeyi sağlar.

---

## Nedir?

- **CookieHelper**: HTTP cookie'lerini yönetmek için kullanılan bir yardımcı sınıftır. Cookie'lerin eklenmesi, okunması ve silinmesi gibi işlemleri kolaylaştırır.
- **SessionHelper**: ASP.NET Core session yönetimi için kullanılan bir yardımcı sınıftır. Session'da veri saklama, okuma, silme ve temizleme gibi işlemleri destekler.

---

## Neden Kullanılır?

- **Cookie ve Session Yönetimi**: Cookie ve session işlemlerini standartlaştırarak, projelerde tutarlı bir yapı sağlar.
- **Asenkron Destek**: Asenkron metotlar ile performansı artırır ve modern uygulama geliştirme standartlarına uyum sağlar.
- **Temiz Kod**: Clean Code prensiplerine uygun olarak geliştirilmiştir.
- **Genişletilebilirlik**: Dependency Injection (DI) ile entegre edilebilir ve genişletilebilir bir yapı sunar.

---

## Avantajları

- **Kolay Kullanım**: Basit ve anlaşılır metotlar ile cookie ve session yönetimi.
- **Güvenlik**: Varsayılan olarak güvenli cookie ayarları (HttpOnly, Secure, SameSite).
- **Esneklik**: JSON serileştirme ve deserileştirme ile her türlü veri tipini destekler.
- **Asenkron İşlemler**: Performansı artıran asenkron metotlar.
- **Loglama ve Hata Yönetimi**: Hata durumlarında loglama ve özel exception'lar.

---

## Projeye Ekleme ve Ayarlar

### 1. Projeye Ekleme

Kütüphaneleri projenize eklemek için aşağıdaki adımları izleyin:

1. **CookieHelper.cs** ve **SessionHelper.cs** dosyalarını projenize ekleyin.
2. Gerekirse, `ICookieHelper` ve `ISessionHelper` interface'lerini kullanarak Dependency Injection (DI) ile entegre edin.

### 2. `Program.cs` Ayarları

`Program.cs` dosyasında gerekli servisleri ekleyin:

```csharp
using Core.ToolKit.SessionManagement;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Session yapılandırması
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout süresi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CookieHelper ve SessionHelper servislerini ekleyin
builder.Services.AddScoped<ICookieHelper, CookieHelper>();
builder.Services.AddScoped<ISessionHelper, SessionHelper>();

var app = builder.Build();

// Session middleware'ini ekleyin
app.UseSession();

app.Run();
```

### 3. `appsettings.json` Ayarları

`appsettings.json` dosyasında cookie ve session ile ilgili ayarları yapılandırabilirsiniz:

```json
{
  "CookieSettings": {
    "DefaultExpirationDays": 7,
    "HttpOnly": true,
    "Secure": true,
    "SameSite": "Strict"
  },
  "SessionSettings": {
    "TimeoutMinutes": 30,
    "CookieName": "MySessionCookie"
  }
}
```

---

## Detaylı Kullanım Örnekleri

### 1. **CookieHelper Kullanımı**

#### Cookie Ekleme
```csharp
public class HomeController : Controller
{
    private readonly ICookieHelper _cookieHelper;

    public HomeController(ICookieHelper cookieHelper)
    {
        _cookieHelper = cookieHelper;
    }

    public async Task<IActionResult> SetCookie()
    {
        var options = _cookieHelper.GetDefaultCookieOptions();
        await _cookieHelper.SetAsync(HttpContext.Response, "UserToken", "12345", options);
        return Ok("Cookie set successfully.");
    }
}
```

#### Cookie Okuma
```csharp
public async Task<IActionResult> GetCookie()
{
    var userToken = await _cookieHelper.GetAsync(HttpContext.Request, "UserToken");
    return Ok($"UserToken: {userToken}");
}
```

#### Cookie Silme
```csharp
public async Task<IActionResult> RemoveCookie()
{
    await _cookieHelper.RemoveAsync(HttpContext.Response, "UserToken");
    return Ok("Cookie removed successfully.");
}
```

---

### 2. **SessionHelper Kullanımı**

#### Session'a Veri Ekleme
```csharp
public class HomeController : Controller
{
    private readonly ISessionHelper _sessionHelper;

    public HomeController(ISessionHelper sessionHelper)
    {
        _sessionHelper = sessionHelper;
    }

    public async Task<IActionResult> SetSession()
    {
        var userData = new { UserId = 1, UserName = "JohnDoe" };
        await _sessionHelper.SetAsync(HttpContext.Session, "UserData", userData);
        return Ok("Session data set successfully.");
    }
}
```

#### Session'dan Veri Okuma
```csharp
public async Task<IActionResult> GetSession()
{
    var userData = await _sessionHelper.GetAsync<dynamic>(HttpContext.Session, "UserData");
    return Ok($"UserData: {userData}");
}
```

#### Session'dan Veri Silme
```csharp
public async Task<IActionResult> RemoveSession()
{
    await _sessionHelper.RemoveAsync(HttpContext.Session, "UserData");
    return Ok("Session data removed successfully.");
}
```

#### Session'u Temizleme
```csharp
public async Task<IActionResult> ClearSession()
{
    await _sessionHelper.ClearAsync(HttpContext.Session);
    return Ok("Session cleared successfully.");
}
```

---

## Örnek Proje Yapısı

```
/MyProject
│
├── /Controllers
│   └── HomeController.cs
│
├── /Services
│   └── CookieHelper.cs
│   └── SessionHelper.cs
│
├── appsettings.json
├── Program.cs
└── Startup.cs
```

---

## Sonuç

Bu kütüphaneler, ASP.NET Core projelerinde cookie ve session yönetimini kolaylaştırmak için tasarlanmıştır. Asenkron metotlar, güvenli ayarlar ve temiz kod yapısı ile büyük ölçekli projelerde rahatlıkla kullanılabilir. Dependency Injection ile entegre edilebilir ve genişletilebilir bir yapı sunar.