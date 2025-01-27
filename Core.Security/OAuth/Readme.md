---

# Core.Security.OAuth

## Nedir?
`Core.Security.OAuth`, OAuth 2.0 protokolünü kullanarak kimlik doğrulama ve yetkilendirme işlemlerini kolayca gerçekleştirmek için tasarlanmış bir .NET kütüphanesidir. Bu kütüphane, OAuth 2.0 akışlarını (Authorization Code, Refresh Token, vb.) destekler ve token yönetimi, kullanıcı bilgileri alımı gibi işlemleri basitleştirir.

## Neden Kullanılır?
- **Kolay Entegrasyon**: OAuth 2.0 akışlarını hızlı ve kolay bir şekilde projelerinize entegre edebilirsiniz.
- **Güvenlik**: Token'ların güvenli bir şekilde yönetilmesini sağlar ve token'ların geçerliliğini otomatik olarak kontrol eder.
- **Esneklik**: Farklı OAuth sağlayıcılarıyla (Google, Facebook, GitHub, vb.) çalışabilir ve özelleştirilebilir yapısı sayesinde farklı senaryolara uyum sağlar.
- **Best Practice'lere Uygun**: Clean code ve async/await prensiplerine uygun olarak tasarlanmıştır.

## Avantajları
- **Token Yönetimi**: Token'ların alınması, yenilenmesi ve geçerliliğinin kontrol edilmesi gibi işlemler otomatik olarak yönetilir.
- **Kullanıcı Bilgileri**: Token kullanarak kullanıcı bilgilerini kolayca alabilirsiniz.
- **Genişletilebilirlik**: Yeni OAuth sağlayıcıları veya özelleştirilmiş akışlar eklemek kolaydır.
- **Hata Yönetimi**: Detaylı hata mesajları ve exception handling mekanizmaları sayesinde sorunları hızlıca tespit edebilirsiniz.

---

## Kurulum ve Yapılandırma

### 1. Projeye Ekleme
Projenize `Core.Security.OAuth` kütüphanesini eklemek için aşağıdaki adımları izleyin:

#### NuGet Paketi Olarak Ekleme
Eğer bu kütüphane bir NuGet paketi olarak yayınlandıysa, aşağıdaki komutla projenize ekleyebilirsiniz:

```bash
dotnet add package Core.Security.OAuth
```

#### Manuel Ekleme
Eğer NuGet paketi olarak yayınlanmadıysa, `OAuthConfiguration.cs` ve `OAuthService.cs` dosyalarını projenize manuel olarak ekleyebilirsiniz.

---

### 2. Yapılandırma Dosyaları

#### `appsettings.json`
OAuth yapılandırması için gerekli bilgileri `appsettings.json` dosyasında tanımlayın:

```json
{
  "OAuthSettings": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "AuthorizationEndpoint": "https://provider.com/oauth/authorize",
    "TokenEndpoint": "https://provider.com/oauth/token",
    "UserInfoEndpoint": "https://provider.com/oauth/userinfo",
    "RedirectUri": "https://yourapp.com/callback",
    "Scopes": ["openid", "profile", "email"]
  }
}
```

---

#### `Program.cs` veya `Startup.cs`
OAuth servisini dependency injection ile projenize ekleyin:

```csharp
using Core.Security.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// OAuth yapılandırmasını appsettings.json'dan yükle
var oauthConfig = builder.Configuration.GetSection("OAuthSettings").Get<OAuthConfiguration>();

// HttpClient ve OAuthService'i dependency injection'a ekle
builder.Services.AddHttpClient();
builder.Services.AddSingleton(oauthConfig);
builder.Services.AddScoped<OAuthService>();

var app = builder.Build();

// Uygulama rotalarını ve middleware'leri burada tanımlayın
app.Run();
```

---

## Detaylı Kullanım Örnekleri

### 1. Authorization URL Oluşturma
Kullanıcıyı OAuth sağlayıcısına yönlendirmek için authorization URL'sini oluşturun:

```csharp
public class AuthController : ControllerBase
{
    private readonly OAuthService _oauthService;

    public AuthController(OAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var authUrl = _oauthService.GetAuthorizationUrl();
        return Redirect(authUrl);
    }
}
```

---

### 2. Authorization Code ile Token Alma
Kullanıcı OAuth sağlayıcısından geri döndüğünde, authorization code'u kullanarak token'ı alın:

```csharp
[HttpGet("callback")]
public async Task<IActionResult> Callback(string code)
{
    try
    {
        var tokenResponse = await _oauthService.ExchangeCodeForTokenAsync(code);
        return Ok(tokenResponse);
    }
    catch (HttpRequestException ex)
    {
        return BadRequest($"Token alınamadı: {ex.Message}");
    }
}
```

---

### 3. Token Yenileme
Eğer token'ın süresi dolmuşsa, refresh token kullanarak yeni bir token alın:

```csharp
public async Task<string> RefreshTokenAsync(string refreshToken)
{
    try
    {
        var newToken = await _oauthService.RefreshAccessTokenAsync(refreshToken);
        return newToken;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception($"Token yenilenemedi: {ex.Message}");
    }
}
```

---

### 4. Kullanıcı Bilgilerini Alma
Token kullanarak kullanıcı bilgilerini alın:

```csharp
public async Task<IActionResult> GetUserInfo(string token)
{
    try
    {
        var userInfo = await _oauthService.GetUserInfoAsync(token);
        return Ok(userInfo);
    }
    catch (HttpRequestException ex)
    {
        return BadRequest($"Kullanıcı bilgileri alınamadı: {ex.Message}");
    }
}
```

---

### 5. Token Geçerliliğini Kontrol Etme
Token'ın geçerli olup olmadığını kontrol edin:

```csharp
public async Task<bool> ValidateToken(string token)
{
    return await _oauthService.ValidateTokenAsync(token);
}
```

---

### 6. Token Süresinin Dolup Dolmadığını Kontrol Etme
Token'ın süresinin dolup dolmadığını kontrol edin:

```csharp
public bool IsTokenExpired(string token)
{
    return _oauthService.IsTokenExpired(token);
}
```

---

## Örnek Proje Yapısı
Örnek bir proje yapısı aşağıdaki gibi olabilir:

```
/YourProject
│
├── /Controllers
│   └── AuthController.cs
│
├── /Models
│   └── OAuthConfiguration.cs
│
├── /Services
│   └── OAuthService.cs
│
├── appsettings.json
├── Program.cs
└── README.md
```

---