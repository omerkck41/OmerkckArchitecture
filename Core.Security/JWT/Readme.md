# JWT ve Opsiyonel Redis 

## 📌 Nedir?
Bu yapı, **JSON Web Token (JWT)** tabanlı kimlik doğrulama sistemini **isteğe bağlı Redis entegrasyonu** ile destekleyen modüler bir altyapıdır. **Redis kullanımı opsiyonel olup**, **Redis olmadan da** çalışabilir. 

JWT, istemci ve sunucu arasındaki kimlik doğrulama işlemlerini güvenli bir şekilde gerçekleştirmek için kullanılan bir JSON tabanlı güvenlik standardıdır.

---

## 🔥 Neden Kullanılır?
Bu yapı, **yetkilendirme işlemlerini** güvenli ve ölçeklenebilir bir şekilde yönetmek için kullanılır. **Redis entegrasyonu**, **blacklist (kara liste)** yönetimini optimize ederek, iptal edilen token'ların anında geçersiz kılınmasını sağlar.

### **Ne Sağlar?**
✔ **Modüler ve esnek yapı** - Redis kullanılabilir veya kullanılmayabilir.  
✔ **Token geçerliliği ve iptal yönetimi** - Redis veya in-memory blacklist mekanizması sağlar.  
✔ **Dağıtık yapı desteği** - Redis kullanıldığında çoklu sunucu ortamlarında geçersiz token'lar anında tanınır.  
✔ **Kolay entegrasyon** - .NET 9.0 ile uyumlu, hızlı şekilde projeye entegre edilebilir.  
✔ **Performans odaklı** - Redis ile token doğrulama işlemleri daha hızlı gerçekleşir.  

---

## 🚀 Nasıl Kullanılır?

### 1️⃣ **Proje Bağımlılıklarını Yükleme**
Öncelikle Redis ve JWT için gerekli bağımlılıkları yükleyelim:

```sh
 dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
 dotnet add package Microsoft.Extensions.Configuration.Json
 dotnet add package StackExchange.Redis
```

---

### 2️⃣ **appsettings.json Ayarları**
JWT ve Redis ile ilgili konfigürasyonları aşağıdaki gibi ekleyin:

```json
{
  "TokenOptions": {
    "Audience": "your-audience",
    "Issuer": "your-issuer",
    "AccessTokenExpiration": 60,
    "SecurityKey": "your-secure-key-should-be-at-least-32-characters-long",
    "RefreshTokenTTL": 7
  },
  "Redis": {
    "Connection": "localhost:6379"
  }
}
```
📌 **Dikkat!** `SecurityKey` değeri **en az 32 karakter uzunluğunda olmalıdır.**

---

### 3️⃣ **Program.cs Konfigürasyonu**
Aşağıdaki kodları `Program.cs` dosyanıza ekleyerek JWT sistemini ve Redis'i yapılandırabilirsiniz:

```csharp
using Core.Security.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

bool useRedis = builder.Configuration.GetValue<bool>("UseRedis");
builder.Services.AddJwtHelper<int, int, int>(builder.Configuration, useRedis: useRedis);

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey))
        };
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```
📌 **Dikkat!** Eğer Redis kullanmak istemiyorsanız `appsettings.json` içine `"UseRedis": false` ekleyebilirsiniz.

---

## 🎯 **JWT Kullanımı**
Aşağıda JWT oluşturma, doğrulama ve iptal etme işlemleri için kullanabileceğiniz örnek kodlar verilmiştir.

### **✅ Token Oluşturma**
```csharp
var tokenHelper = serviceProvider.GetRequiredService<ITokenHelper<int, int, int>>();
var user = new User<int> { Id = 1, Email = "test@example.com", FirstName = "Test", LastName = "User" };
var claims = new List<OperationClaim<int>> { new(1, "Admin") };
var token = tokenHelper.CreateToken(user, claims);

Console.WriteLine($"Oluşturulan Token: {token.Token}");
```

### **✅ Token Doğrulama**
```csharp
bool isValid = tokenHelper.ValidateToken(token.Token);
Console.WriteLine(isValid ? "Token geçerli" : "Token geçersiz");
```

### **✅ Token İptal Etme (Blacklist)**
```csharp
tokenHelper.RevokeToken(token.Token);
Console.WriteLine("Token iptal edildi");
```

📌 **Eğer Redis kullanılıyorsa, iptal edilen token anında geçersiz sayılır!**

---

## 🎯 **Özet ve Sonuç**
Bu yapı sayesinde JWT kimlik doğrulama sisteminizi **isteğe bağlı Redis desteğiyle** daha güvenli ve performanslı hale getirebilirsiniz. 

✅ **Esnek kullanım** – Redis kullanılabilir veya in-memory çalışabilir.  
✅ **Yüksek performans** – Redis ile token doğrulama işlemleri hızlandırılır.  
✅ **Kolay entegrasyon** – .NET 9.0 projelerinde hızlıca kullanılabilir.  

