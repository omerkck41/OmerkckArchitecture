---

# **Core.Security Katmanı**

`Core.Security` katmanı, uygulamalarınızda güvenlikle ilgili işlemleri kolaylaştırmak için geliştirilmiş bir .NET kütüphanesidir. Bu katman, kimlik doğrulama, yetkilendirme, şifreleme, token yönetimi ve çok faktörlü kimlik doğrulama (MFA) gibi temel güvenlik işlemlerini destekler. Aşağıda, bu katmanın bileşenleri ve nasıl kullanılacağına dair detaylı bilgiler bulunmaktadır.

---

## **Bileşenler**

### **1. JWT Token Yönetimi**
- **AccessToken:** JWT tabanlı access token'ları temsil eder.
- **RefreshToken:** Kullanıcı oturumlarını uzun süreli olarak yönetmek için kullanılan refresh token'ları temsil eder.
- **TokenOptions:** Token ayarlarını yapılandırmak için kullanılır.
- **JwtHelper:** JWT token'ları oluşturma, doğrulama ve yenileme işlemlerini yönetir.
- **TokenBlacklist:** İptal edilen token'ları takip eder.

### **2. Çok Faktörlü Kimlik Doğrulama (MFA)**
- **IMfaService:** MFA işlemlerini yönetmek için kullanılan interface.
- **MfaService:** Authenticator, TOTP, e-posta ve SMS tabanlı doğrulama işlemlerini gerçekleştirir.
- **TotpService:** Zaman tabanlı tek kullanımlık şifre (TOTP) üretimi ve doğrulamasını yönetir.

### **3. Şifreleme ve Hashing**
- **HashingService:** Şifre hash'leme ve doğrulama işlemlerini yönetir.
- **SecurityKeyHelper:** Güvenlik anahtarları oluşturur.
- **SigningCredentialsHelper:** JWT token'ları için imzalama kimlik bilgileri oluşturur.

### **4. OAuth 2.0**
- **OAuthConfiguration:** OAuth 2.0 yapılandırma ayarlarını temsil eder.
- **OAuthService:** OAuth 2.0 akışlarını yönetir (Authorization Code, Refresh Token, vb.).

### **5. Token İptal ve Doğrulama**
- **TokenBlacklist:** İptal edilen token'ları takip eder.
- **TokenValidator:** Token'ların geçerliliğini doğrular.

### **6. Kullanıcı Doğrulama ve Kayıt**
- **UserForLoginDto:** Kullanıcı girişi için kullanılan veri transfer nesnesi (DTO).
- **UserForRegisterDto:** Kullanıcı kaydı için kullanılan veri transfer nesnesi (DTO).
- **UserForLoginValidator:** Kullanıcı girişi için doğrulama kurallarını içerir.
- **UserForRegisterValidator:** Kullanıcı kaydı için doğrulama kurallarını içerir.

---

## **Neden Kullanılır?**

- **Güvenlik:** Kullanıcı kimlik doğrulama, yetkilendirme ve şifreleme işlemlerini güvenli bir şekilde yönetir.
- **Esneklik:** Farklı güvenlik yöntemlerini (JWT, OAuth, MFA) destekler.
- **Kolay Entegrasyon:** Basit ve anlaşılır bir API ile projelerinize kolayca entegre edilebilir.
- **Genişletilebilirlik:** Yeni güvenlik yöntemleri eklemek veya mevcut yöntemleri özelleştirmek kolaydır.

---

## **Avantajları**

- **Token Yönetimi:** JWT tabanlı token'ların oluşturulması, doğrulanması ve yenilenmesi kolaydır.
- **Çok Faktörlü Kimlik Doğrulama:** Authenticator, TOTP, e-posta ve SMS tabanlı doğrulama seçenekleri sunar.
- **Şifreleme ve Hashing:** Şifrelerin güvenli bir şekilde hash'lenmesini ve doğrulanmasını sağlar.
- **OAuth 2.0 Desteği:** OAuth 2.0 akışlarını kolayca yönetmenizi sağlar.
- **Token İptal Listesi:** İptal edilen token'ları takip eder ve güvenliği artırır.

---

## **Kurulum ve Yapılandırma**

### **1. Projeye Ekleme**

Kütüphaneyi projenize eklemek için aşağıdaki adımları izleyin:

1. **ServiceCollectionExtensions Kullanımı:**
   Projenizin `Program.cs` veya `Startup.cs` dosyasında, gerekli servisleri ekleyin:
   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       // JWT Token Yönetimi
       services.AddJwtHelper<Guid, int, Guid>(Configuration);

       // MFA Servisleri
       services.AddSingleton<TotpService>();
       services.AddScoped<IMfaService, MfaService>();

       // OAuth Servisleri
       var oauthConfig = Configuration.GetSection("OAuthSettings").Get<OAuthConfiguration>();
       services.AddSingleton(oauthConfig);
       services.AddScoped<OAuthService>();

       // Hashing Servisleri
       services.AddScoped<IHashingService, HashingService>();
   }
   ```

### **2. Yapılandırma Dosyaları**

`appsettings.json` dosyanıza aşağıdaki gibi ayarları ekleyin:
```json
{
  "TokenOptions": {
    "Audience": "YourAudience",
    "Issuer": "YourIssuer",
    "AccessTokenExpiration": 60, // Dakika cinsinden
    "SecurityKey": "YourSuperSecretKey", // En az 16 karakter uzunluğunda
    "RefreshTokenTTL": 7 // Gün cinsinden
  },
  "OAuthSettings": {
    "ClientId": "YourClientId",
    "ClientSecret": "YourClientSecret",
    "AuthorizationEndpoint": "https://provider.com/oauth/authorize",
    "TokenEndpoint": "https://provider.com/oauth/token",
    "UserInfoEndpoint": "https://provider.com/oauth/userinfo",
    "RedirectUri": "https://yourapp.com/callback",
    "Scopes": ["openid", "profile", "email"]
  }
}
```

---

## **Detaylı Kullanım Örnekleri**

### **1. JWT Token Oluşturma ve Doğrulama**
```csharp
public class AuthService
{
    private readonly ITokenHelper<Guid, int, Guid> _tokenHelper;

    public AuthService(ITokenHelper<Guid, int, Guid> tokenHelper)
    {
        _tokenHelper = tokenHelper;
    }

    public async Task<AccessToken> CreateAccessTokenAsync(User<Guid> user, IList<OperationClaim<int>> operationClaims)
    {
        return await _tokenHelper.CreateTokenAsync(user, operationClaims);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        return await _tokenHelper.ValidateTokenAsync(token);
    }
}
```

### **2. Çok Faktörlü Kimlik Doğrulama (MFA)**
```csharp
public class MfaController : ControllerBase
{
    private readonly IMfaService _mfaService;

    public MfaController(IMfaService mfaService)
    {
        _mfaService = mfaService;
    }

    [HttpGet("generate-totp")]
    public async Task<IActionResult> GenerateTotpCode(string secretKey)
    {
        var code = await _mfaService.GenerateTotpCodeAsync(secretKey);
        return Ok(new { Code = code });
    }

    [HttpPost("validate-totp")]
    public async Task<IActionResult> ValidateTotpCode(string inputCode, string secretKey)
    {
        var isValid = await _mfaService.ValidateTotpCodeAsync(inputCode, secretKey);
        return Ok(new { IsValid = isValid });
    }
}
```

### **3. OAuth 2.0 ile Kimlik Doğrulama**
```csharp
public class OAuthController : ControllerBase
{
    private readonly OAuthService _oauthService;

    public OAuthController(OAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var authUrl = _oauthService.GetAuthorizationUrl();
        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string code)
    {
        var tokenResponse = await _oauthService.ExchangeCodeForTokenAsync(code);
        return Ok(tokenResponse);
    }
}
```

---

## **Örnek Proje Yapısı**

```plaintext
Core.Security/
├── JWT/
│   ├── AccessToken.cs
│   ├── ITokenHelper.cs
│   ├── JwtHelper.cs
│   ├── RefreshToken.cs
│   ├── TokenOptions.cs
│   ├── ServiceCollectionExtensions.cs
│   ├── TokenBlacklist.cs
│   ├── IRefreshTokenRepository.cs
│   └── RefreshTokenRepository.cs
├── MFA/
│   ├── IMfaService.cs
│   ├── MfaService.cs
│   ├── TotpService.cs
├── OAuth/
│   ├── OAuthConfiguration.cs
│   ├── OAuthService.cs
├── Hashing/
│   ├── HashingService.cs
│   ├── IHashingService.cs
├── Validators/
│   ├── UserForLoginValidator.cs
│   ├── UserForRegisterValidator.cs
└── README.md
```

---

## **Sonuç**

`Core.Security` katmanı, uygulamalarınızda güvenlikle ilgili işlemleri kolaylaştırmak için geliştirilmiştir. Bu katman, JWT token yönetimi, çok faktörlü kimlik doğrulama, OAuth 2.0 ve şifreleme gibi temel güvenlik işlemlerini destekler. Yukarıdaki adımları takip ederek, bu katmanı projenize entegre edebilir ve kullanmaya başlayabilirsiniz.