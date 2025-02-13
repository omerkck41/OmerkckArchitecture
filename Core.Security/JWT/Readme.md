# JWT Yardımcı Kütüphanesi README

## 📌 **Nedir?**  
Bu kütüphane, .NET projelerinde kimlik doğrulama (Authentication) ve yetkilendirme (Authorization) işlemleri için JWT (JSON Web Token) yönetimi sağlar. Ayrıca, Redis veya In-Memory kullanarak token kara liste (blacklist) yönetimi yapabilir.

---

## 💡 **Neden Kullanılır?**  
- ✅ **Güvenlik:** Yetkilendirme süreçlerinde geçersiz kılınmış token’ları hızlıca engelleyebilirsiniz.  
- ✅ **Esneklik:** Redis veya In-Memory kullanarak çeşitli senaryolara uygun kara liste yönetimi sağlar.  
- ✅ **Performans:** Redis kullanımı ile yüksek ölçekli projelerde hızlı kara liste kontrolü yapılır.

---

## 🚀 **Avantajları Nelerdir?**  
- 📌 **Generic Yapı:** `TUserId`, `TOperationClaimId`, `TRefreshTokenId` gibi generic tiplerle esnek kullanım.  
- 📌 **Kolay Entegrasyon:** `IServiceCollection` genişletmesi ile kolay DI yapılandırması.  
- 📌 **Genişletilebilirlik:** `ITokenHelper`, `ITokenBlacklistManager` gibi arayüzlerle kolay özelleştirme.  

---

## ⚙️ **Projeye Nasıl Eklenir?**  
### 1️⃣ **appsettings.json** Düzeni:
```json
{
  "TokenOptions": {
    "Audience": "example.com",
    "Issuer": "example.com",
    "AccessTokenExpiration": 60,
    "SecurityKey": "super_secure_secret_key",
    "RefreshTokenTTL": 7
  },
  "Redis": {
    "Connection": "localhost:6379"
  }
}
```

### 2️⃣ **Program.cs** Yapılandırması:
```csharp
builder.Services.AddJwtHelper<int, int, int>(
    builder.Configuration,
    useRedis: true // Redis kullanımı aktif
);
```

---

## 📝 **Kullanım Örnekleri:**  
### ✅ **Access Token ve Refresh Token Oluşturma:**
```csharp
var accessToken = jwtHelper.CreateToken(user, operationClaims);
var refreshToken = jwtHelper.CreateRefreshToken(user, ipAddress);
```

### ✅ **Token İptali (Blacklist):**
```csharp
jwtHelper.RevokeToken(accessToken.Token, user.Id.ToString(), TimeSpan.FromHours(1));
```

### ✅ **Token Geçerliliği Kontrolü:**
```csharp
bool isValid = jwtHelper.ValidateToken(accessToken.Token);
```

### ✅ **Kullanıcı Tüm Oturumlarını İptal Etme:**
```csharp
jwtHelper.RevokeToken("all_sessions", user.Id.ToString(), TimeSpan.FromDays(1));
```

---

## 🎯 **Sonuç:**  
Bu JWT yardımcı kütüphanesi, güvenli, genişletilebilir ve performanslı bir token yönetimi sunar. Redis veya In-Memory seçenekleriyle projelerinizin ihtiyaçlarına göre esnek çözümler sunar. 🚀

