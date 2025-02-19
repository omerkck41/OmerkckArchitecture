---

# Core.Security.MFA

Bu kütüphane, **Multi-Factor Authentication (MFA)** işlemlerini kolayca uygulayabilmeniz için geliştirilmiştir. TOTP (Time-Based One-Time Password), Authenticator, E-posta ve SMS tabanlı doğrulama işlemlerini destekler. Büyük ölçekli projelerde rahatça kullanılabilir ve genişletilebilir bir yapıya sahiptir.

---

## Nedir?

**Core.Security.MFA**, kullanıcıların kimlik doğrulama sürecini güçlendirmek için kullanılan bir kütüphanedir. Aşağıdaki özellikleri sunar:

- **TOTP (Time-Based One-Time Password)**: Zaman tabanlı tek kullanımlık şifre üretimi ve doğrulaması.
- **Authenticator**: 6 haneli rastgele kod üretimi ve doğrulaması.
- **E-posta ve SMS Doğrulama**: E-posta veya SMS ile gönderilecek 6 haneli rastgele kod üretimi ve doğrulaması.

---

## Neden Kullanılır?

- **Güvenlik Artışı**: MFA, kullanıcı hesaplarını yetkisiz erişimlere karşı korur.
- **Esneklik**: Farklı doğrulama yöntemleri (TOTP, Authenticator, E-posta, SMS) destekler.
- **Kolay Entegrasyon**: Basit ve anlaşılır bir API ile projelerinize kolayca entegre edilebilir.
- **Genişletilebilirlik**: Yeni doğrulama yöntemleri eklenebilir.

---

## Avantajları

- **Clean Code**: Best practice'lere uygun, temiz ve anlaşılır kod yapısı.
- **Async Destek**: Tüm metotlar asenkron olarak çalışır.
- **Yapılandırılabilirlik**: TOTP zaman adımı (time step) gibi parametreler yapılandırılabilir.
- **Hata Yönetimi**: Tüm metotlarda hata yönetimi ve loglama desteği.

---

## Projeye Ekleme ve Ayarlar

### 1. **Projeye Ekleme**

Kütüphaneyi projenize eklemek için aşağıdaki adımları izleyin:

1. **Core.Security.MFA** klasörünü projenize ekleyin.
2. `IMfaService`, `MfaService` ve `ITotpService` sınıflarını kullanmak için gerekli bağımlılıkları ekleyin.

### 2. **Program.cs Ayarları**

`Program.cs` dosyasında `MfaService` ve `TotpService` sınıflarını dependency injection ile kaydedin:

```csharp
using Core.Security.MFA;

var builder = WebApplication.CreateBuilder(args);

// Servisleri kaydet
builder.Services.AddSingleton<ITotpService, TotpService>();
builder.Services.AddSingleton<IMfaService, MfaService>();

var app = builder.Build();

// Uygulama başlatma
app.Run();
```

### 3. **appsettings.json Ayarları**

`appsettings.json` dosyasında TOTP için gerekli ayarları ekleyin:

```json
{
  "MfaSettings": {
    "TotpTimeStep": 30 // TOTP zaman adımı (saniye cinsinden)
  }
}
```

---

## Detaylı Kullanım Örnekleri

### 1. **Authenticator Kodu Üretme ve Doğrulama**

```csharp
public class AuthController : ControllerBase
{
    private readonly IMfaService _mfaService;

    public AuthController(IMfaService mfaService)
    {
        _mfaService = mfaService;
    }

    [HttpGet("generate-authenticator-code")]
    public async Task<IActionResult> GenerateAuthenticatorCode()
    {
        var code = await _mfaService.GenerateAuthenticatorCodeAsync();
        return Ok(new { Code = code });
    }

    [HttpPost("validate-authenticator-code")]
    public async Task<IActionResult> ValidateAuthenticatorCode([FromBody] string inputCode, [FromBody] string expectedCode)
    {
        var isValid = await _mfaService.ValidateAuthenticatorCodeAsync(inputCode, expectedCode);
        return Ok(new { IsValid = isValid });
    }
}
```

### 2. **TOTP Kodu Üretme ve Doğrulama**

```csharp
public class TotpController : ControllerBase
{
    private readonly IMfaService _mfaService;

    public TotpController(IMfaService mfaService)
    {
        _mfaService = mfaService;
    }

    [HttpGet("generate-totp-code")]
    public async Task<IActionResult> GenerateTotpCode([FromQuery] string secretKey)
    {
        var code = await _mfaService.GenerateTotpCodeAsync(secretKey);
        return Ok(new { Code = code });
    }

    [HttpPost("validate-totp-code")]
    public async Task<IActionResult> ValidateTotpCode([FromBody] string inputCode, [FromBody] string secretKey)
    {
        var isValid = await _mfaService.ValidateTotpCodeAsync(inputCode, secretKey);
        return Ok(new { IsValid = isValid });
    }
}
```

### 3. **E-posta ve SMS Kodu Üretme ve Doğrulama**

```csharp
public class NotificationController : ControllerBase
{
    private readonly IMfaService _mfaService;

    public NotificationController(IMfaService mfaService)
    {
        _mfaService = mfaService;
    }

    [HttpGet("generate-email-code")]
    public async Task<IActionResult> GenerateEmailCode()
    {
        var code = await _mfaService.GenerateEmailCodeAsync();
        return Ok(new { Code = code });
    }

    [HttpPost("validate-email-code")]
    public async Task<IActionResult> ValidateEmailCode([FromBody] string inputCode, [FromBody] string expectedCode)
    {
        var isValid = await _mfaService.ValidateEmailCodeAsync(inputCode, expectedCode);
        return Ok(new { IsValid = isValid });
    }
}
```
