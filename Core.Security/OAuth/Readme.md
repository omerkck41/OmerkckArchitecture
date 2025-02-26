---

# OAuth 2.0 Servisi

Bu proje, **OAuth 2.0** protokolünü kullanarak kimlik doğrulama ve yetkilendirme işlemlerini gerçekleştirmek için geliştirilmiş bir servistir. Facebook, Google, GitHub gibi popüler OAuth sağlayıcılarıyla entegre çalışabilir ve kullanıcıların üçüncü taraf hesaplarıyla giriş yapmasını sağlar.

---

## **Özellikler**

- **OAuth 2.0 Akışı:** Yetkilendirme kodu (authorization code) ve erişim token'ı (access token) yönetimi.
- **Token Doğrulama:** JWT token'larının imza ve süre (expiration) kontrolü.
- **Token Yenileme:** Refresh token'ları kullanarak erişim token'larını yenileme.
- **Kullanıcı Bilgisi Çekme:** Erişim token'ı ile kullanıcı bilgilerini çekme.
- **JSON Tabanlı Yapılandırma:** Yapılandırma bilgileri `appsettings.json` dosyasından alınır.
- **SOLID Prensipleri:** Modüler, genişletilebilir ve test edilebilir bir yapı.
- **Dependency Injection:** Bağımlılıklar DI üzerinden yönetilir.

---

## **Neden Kullanılmalı?**

- **Güvenlik:** Kullanıcıların şifrelerini uygulamanıza vermesine gerek yoktur. Sadece erişim token'ı paylaşılır.
- **Kullanıcı Dostu:** Kullanıcılar, yeni bir hesap oluşturmak yerine mevcut hesaplarıyla (Facebook, Google) hızlıca giriş yapabilir.
- **Standart ve Yaygın:** OAuth 2.0, Facebook, Google, GitHub gibi birçok büyük platform tarafından desteklenir.
- **Esneklik:** Farklı OAuth sağlayıcılarıyla kolayca entegre olabilir ve yeni özellikler eklenebilir.

---

## **Kurulum ve Kullanım**

### **1. Yapılandırma**
`appsettings.json` dosyasına OAuth yapılandırma bilgilerini ekleyin:

```json
{
  "OAuthSettings": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "AuthorizationEndpoint": "https://provider.com/oauth/authorize",
    "TokenEndpoint": "https://provider.com/oauth/token",
    "RedirectUri": "https://yourapp.com/auth/callback",
    "UserInfoEndpoint": "https://provider.com/oauth/userinfo",
    "Scopes": [ "email", "profile" ]
  }
}
```

### **2. Dependency Injection Ayarları**
`Startup.cs` veya `Program.cs` dosyasında servisleri kaydedin:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // OAuthSettings'i appsettings.json'dan yükle
    services.Configure<OAuthSettings>(Configuration.GetSection("OAuthSettings"));

    // OAuthConfiguration'ı oluştur ve DI'ye kaydet
    services.AddSingleton(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<OAuthSettings>>().Value;
        return new OAuthConfiguration(settings);
    });

    // HttpClient ve OAuthService'i DI'ye kaydet
    services.AddHttpClient<IOAuthService, OAuthService>();
}
```

### **3. Örnek Login Yönetimi**
Aşağıda, OAuth servisini kullanarak bir login işlemi gerçekleştiren örnek bir controller bulunmaktadır:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IOAuthService _oauthService;

    public AuthController(IOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    // Kullanıcıyı OAuth sağlayıcısının giriş sayfasına yönlendir
    [HttpGet("login")]
    public IActionResult Login()
    {
        var authorizationUrl = _oauthService.GetAuthorizationUrl();
        return Redirect(authorizationUrl);
    }

    // OAuth sağlayıcısından gelen yetkilendirme kodunu işle
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string code)
    {
        // Yetkilendirme kodunu kullanarak erişim token'ı al
        var token = await _oauthService.ExchangeCodeForTokenAsync(code);

        // Erişim token'ı ile kullanıcı bilgilerini çek
        var userInfo = await _oauthService.GetUserInfoAsync(token);

        // Kullanıcı bilgilerini işle ve oturum aç
        return Ok(userInfo);
    }
}
```

---

## **Avantajlar**

- **Modüler Yapı:** Farklı OAuth sağlayıcılarıyla kolayca entegre olabilir.
- **Güvenlik:** Token'ların imza ve süre kontrolü otomatik olarak yapılır.
- **Esneklik:** JSON tabanlı yapılandırma ile farklı ortamlar için farklı ayarlar kolayca yönetilir.
- **Test Edilebilirlik:** DI ve interface'ler sayesinde unit test ve integration test yapmak kolaydır.

---

## **Örnek Senaryo: Facebook ile Giriş**

1. Kullanıcı, uygulamanızda **"Facebook ile Giriş Yap"** butonuna tıklar.
2. Uygulamanız, kullanıcıyı Facebook'un giriş sayfasına yönlendirir.
3. Kullanıcı, Facebook'ta giriş yapar ve uygulamanıza erişim izni verir.
4. Facebook, uygulamanıza bir yetkilendirme kodu gönderir.
5. Uygulamanız, bu kodu kullanarak bir erişim token'ı alır.
6. Uygulamanız, erişim token'ı ile Facebook'tan kullanıcı bilgilerini çeker.
7. Kullanıcı bilgileri kullanılarak oturum açılır veya kullanıcı kaydedilir.

---