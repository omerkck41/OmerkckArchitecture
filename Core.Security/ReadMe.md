# Core.Security Modülü - Kapsamlı Entegrasyon ve Kullanım Rehberi

## **Giriş**
Core.Security, projelerdeki güvenlik ihtiyaçlarını karşılamak için tasarlanmış, modüler ve genişletilebilir bir kütüphanedir. JWT tabanlı kimlik doğrulama, MFA, token iptali, hashing, HTTP güvenlik başlıkları, OAuth/OpenID Connect desteği ve secrets yönetimi gibi birçok güvenlik aracını içerir.

Bu rehber, Core.Security modülünü projeye **submodule** olarak eklemeyi, gerekli **NuGet paketlerini kurmayı**, **Program.cs** ve **appsettings.json** yapılandırmalarını tamamlamayı ve tüm modüllerin kullanımını örneklerle açıklar.

---

## **1. Core.Security Modülünü Projeye Ekleme**

### **Submodule Ekleme**
Core.Security modülünü projeye **submodule** olarak eklemek için aşağıdaki komutu kullanın:

```bash
git submodule add <Core.Security Git Repository URL> Core.Security
```

### **Proje Referansı Ekleme**
1. **Visual Studio** kullanıyorsanız:
   - Projeye sağ tıklayın ve **Add -> Existing Project** seçeneği ile **Core.Security.csproj** dosyasını ekleyin.
   - Ana projeye sağ tıklayın ve **Add Reference** üzerinden **Core.Security**'i referans olarak ekleyin.

2. **.NET CLI** kullanıyorsanız:
   ```bash
   dotnet add reference Core.Security/Core.Security.csproj
   ```

---

## **2. Gerekli NuGet Paketlerinin Kurulumu**
Core.Security modülünün doğru çalışması için aşağıdaki NuGet paketlerinin projeye eklenmesi gerekir:

```bash
Install-Package Microsoft.Extensions.Configuration.Binder
Install-Package Microsoft.AspNetCore.Authentication.JwtBearer
Install-Package FluentValidation
Install-Package Microsoft.IdentityModel.Tokens
Install-Package System.Security.Cryptography
```

### **Paketlerin Görevleri:**
- **Microsoft.Extensions.Configuration.Binder**: `appsettings.json` yapılandırmalarını sınıflara bağlamak için kullanılır.
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT tabanlı kimlik doğrulama sağlar.
- **Microsoft.IdentityModel.Tokens**: Token doğrulama ve güvenlik anahtarlarını yönetir.
- **FluentValidation**: DTO'lar için doğrulama kuralları tanımlar.
- **System.Security.Cryptography**: Hashing ve güvenli kod üretme için kullanılır.

---

## **3. Program.cs Yapılandırması**
`Program.cs` dosyasına Core.Security modülünü dahil etmek için aşağıdaki servis kayıtlarını ekleyin:

```csharp
using Core.Security.EmailAuthenticator;
using Core.Security.Encryption;
using Core.Security.Hashing;
using Core.Security.JWT;
using Core.Security.MFA;
using Core.Security.TokenRevocation;
using Core.Security.Secrets;
using Core.Security.Headers;

var builder = WebApplication.CreateBuilder(args);

// JWT Token Options Configuration
builder.Services.Configure<TokenOptions>(builder.Configuration.GetSection("TokenOptions"));

// Core.Security Servis Kayıtları
builder.Services.AddSingleton<IEmailAuthenticatorHelper, EmailAuthenticatorHelper>();
builder.Services.AddSingleton<IMfaService, MfaService>();
builder.Services.AddSingleton<TokenBlacklist>();
builder.Services.AddSingleton<SecretsManager>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey)
        };
    });

var app = builder.Build();

// Middleware'ler
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

---

## **4. appsettings.json Yapılandırması**
`appsettings.json` dosyasına JWT yapılandırmasını ekleyin:

```json
{
  "TokenOptions": {
    "Audience": "YourAudience",
    "Issuer": "YourIssuer",
    "AccessTokenExpiration": 30,
    "SecurityKey": "YourSuperSecretKey12345"
  }
}
```

### **Parametrelerin Açıklaması:**
- **Audience**: Token'ın hedef kitlesini belirtir.
- **Issuer**: Token'ı oluşturan sunucuyu belirtir.
- **AccessTokenExpiration**: Token'ın geçerlilik süresi (dakika cinsinden).
- **SecurityKey**: Token'ı imzalamak için kullanılan gizli anahtar.

---

## **5. Tüm Modüllerin Kullanımı ve Örnekleri**

### **5.1. DTO'lar (Data Transfer Objects)**
```csharp
var loginDto = new UserForLoginDto
{
    Email = "user@example.com",
    Password = "Password123!"
};
```

### **5.2. EmailAuthenticator Kullanımı**
```csharp
var emailHelper = new EmailAuthenticatorHelper();
string activationCode = await emailHelper.CreateEmailActivationCodeAsync();
bool isValid = await emailHelper.ValidateActivationCodeAsync("123456", activationCode);
```

### **5.3. Encryption Kullanımı**
```csharp
var securityKey = SecurityKeyHelper.CreateSecurityKey("YourSecretKey");
var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
```

### **5.4. Hashing Kullanımı**
```csharp
var passwordHasher = new PasswordHasher();
var (hash, salt) = await passwordHasher.CreatePasswordHashAsync("Password123!");
bool isVerified = await passwordHasher.VerifyPasswordHashAsync("Password123!", hash, salt);
```

### **5.5. JWT Kullanımı**
```csharp
var jwtHelper = new JwtHelper(configuration);
var token = await jwtHelper.CreateTokenAsync(user, operationClaims);
Console.WriteLine(token.Token);
```

### **5.6. HTTP Güvenlik Başlıkları Kullanımı**
```csharp
app.UseMiddleware<SecurityHeadersMiddleware>();
```

### **5.7. MFA Kullanımı**
```csharp
var mfaService = new MfaService();
string code = await mfaService.GenerateAuthenticatorCodeAsync();
bool isValid = await mfaService.ValidateAuthenticatorCodeAsync("123456", code);
```

### **5.8. Token İptali (Revocation) Kullanımı**
```csharp
var tokenBlacklist = new TokenBlacklist();
tokenBlacklist.RevokeToken("token123");
bool isRevoked = tokenBlacklist.IsTokenRevoked("token123");
```

### **5.9. Secrets Yönetimi Kullanımı**
```csharp
var secretsManager = new SecretsManager();
secretsManager.AddSecret("ApiKey", "SuperSecretKey12345");
string apiKey = secretsManager.GetSecret("ApiKey");
```

---

## **Sonuç**
Bu rehber ile Core.Security modülünü eksiksiz bir şekilde projeye ekleyebilir, gerekli NuGet paketlerini yükleyip yapılandırmaları tamamlayarak tüm güvenlik özelliklerini kullanabilirsiniz.
Core.Security, modüler ve genişletilebilir yapısıyla büyük projelerde tüm güvenlik ihtiyaçlarınızı karşılayacaktır.
