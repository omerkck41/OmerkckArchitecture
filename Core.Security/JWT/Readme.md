# JWT Authentication and Token Management Library

## Nedir?

Bu kütüphane, JWT (JSON Web Token) tabanlı kimlik doğrulama ve token yönetimi için geliştirilmiş bir çözümdür. Kullanıcıların kimlik doğrulaması, erişim token'ları oluşturma, refresh token'ları yönetme ve token'ların geçerliliğini kontrol etme gibi işlemleri kolayca gerçekleştirmenizi sağlar. Ayrıca, Redis kullanarak token'ların kara listeye alınması ve refresh token'ların yönetimi gibi özellikleri de destekler.

## Neden Kullanılır?

- **Güvenlik**: JWT token'ları, kullanıcı kimlik doğrulaması ve yetkilendirme işlemlerinde güvenli bir yöntem sunar.
- **Esneklik**: Hem in-memory hem de Redis tabanlı token yönetimi seçenekleri sunar.
- **Kolay Entegrasyon**: .NET projelerine kolayca entegre edilebilir ve yapılandırılabilir.
- **Token Yönetimi**: Refresh token'ların yönetimi, token'ların iptal edilmesi ve süresi dolmuş token'ların temizlenmesi gibi işlemleri otomatikleştirir.

## Avantajları

- **Redis Desteği**: Redis kullanarak token'ların kara listeye alınması ve refresh token'ların yönetimi gibi işlemleri hızlı ve etkili bir şekilde gerçekleştirir.
- **Genişletilebilirlik**: Farklı token yönetim stratejileri ve depolama seçenekleri ile genişletilebilir.
- **Entegrasyon Kolaylığı**: .NET Core projelerine kolayca entegre edilebilir ve yapılandırılabilir.

## Projeye Ekleme ve Yapılandırma

### 1. Projeye Ekleme

Öncelikle, kütüphaneyi projenize eklemek için `ServiceCollectionExtensions` sınıfını kullanarak gerekli servisleri kaydedin.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Diğer servisler...

    // JWT ve Redis tabanlı token hizmetlerini ekleyin
    services.AddJwtHelper<Guid, int, Guid>(Configuration, useRedis: true);
}
```

### 2. `appsettings.json` Yapılandırması

`appsettings.json` dosyasına JWT token yapılandırma seçeneklerini ekleyin.

```json
{
  "TokenOptions": {
    "Audience": "YourAudience",
    "Issuer": "YourIssuer",
    "AccessTokenExpiration": 60,
    "SecurityKey": "YourSuperSecretKey",
    "RefreshTokenTTL": 7
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### 3. `Program.cs` veya `Startup.cs` Yapılandırması

`Program.cs` veya `Startup.cs` dosyasında JWT servislerini kaydedin.

```csharp
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Diğer servisler...

        // JWT ve Redis tabanlı token hizmetlerini ekleyin
        services.AddJwtHelper<Guid, int, Guid>(Configuration, useRedis: true);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Diğer middleware'ler...
    }
}
```

## Detaylı Kullanım Örnekleri

### 1. Access Token Oluşturma

Kullanıcı için bir access token oluşturmak için `CreateTokenAsync` metodunu kullanın.

```csharp
public class AuthService
{
    private readonly ITokenHelper<Guid, int, Guid> _tokenHelper;

    public AuthService(ITokenHelper<Guid, int, Guid> tokenHelper)
    {
        _tokenHelper = tokenHelper;
    }

    public async Task<AccessToken> CreateTokenAsync(User<Guid> user, IList<OperationClaim<int>> operationClaims)
    {
        return await _tokenHelper.CreateTokenAsync(user, operationClaims);
    }
}
```

### 2. Refresh Token Oluşturma

Kullanıcı için bir refresh token oluşturmak için `CreateRefreshTokenAsync` metodunu kullanın.

```csharp
public async Task<RefreshToken<Guid, Guid>> CreateRefreshTokenAsync(User<Guid> user, string ipAddress)
{
    return await _tokenHelper.CreateRefreshTokenAsync(user, ipAddress);
}
```

### 3. Token Yenileme

Mevcut bir refresh token kullanarak yeni bir access token oluşturmak için `RefreshTokenAsync` metodunu kullanın.

```csharp
public async Task<AccessToken> RefreshTokenAsync(User<Guid> user, IList<OperationClaim<int>> operationClaims, string ipAddress)
{
    return await _tokenHelper.RefreshTokenAsync(user, operationClaims, ipAddress);
}
```

### 4. Token Geçerliliğini Kontrol Etme

Bir token'ın geçerliliğini kontrol etmek için `ValidateTokenAsync` metodunu kullanın.

```csharp
public async Task<bool> ValidateTokenAsync(string token)
{
    return await _tokenHelper.ValidateTokenAsync(token);
}
```

### 5. Token İptal Etme

Bir token'ı iptal etmek için `RevokeTokenAsync` metodunu kullanın.

```csharp
public async Task RevokeTokenAsync(string token)
{
    await _tokenHelper.RevokeTokenAsync(token);
}
```

### 6. Token'dan Claim'leri Alma

Bir token'dan claim'leri almak için `GetClaimsFromTokenAsync` metodunu kullanın.

```csharp
public async Task<IEnumerable<Claim>> GetClaimsFromTokenAsync(string token)
{
    return await _tokenHelper.GetClaimsFromTokenAsync(token);
}
```

### 7. Token'dan Kullanıcı ID'sini Alma

Bir token'dan kullanıcı ID'sini almak için `GetUserIdFromTokenAsync` metodunu kullanın.

```csharp
public async Task<Guid> GetUserIdFromTokenAsync(string token)
{
    return await _tokenHelper.GetUserIdFromTokenAsync(token);
}
```

### 8. Token'ın Son Kullanma Tarihini Alma

Bir token'ın son kullanma tarihini almak için `GetExpirationDateFromTokenAsync` metodunu kullanın.

```csharp
public async Task<DateTime> GetExpirationDateFromTokenAsync(string token)
{
    return await _tokenHelper.GetExpirationDateFromTokenAsync(token);
}
```

## Sonuç

Bu kütüphane, JWT tabanlı kimlik doğrulama ve token yönetimi için kapsamlı bir çözüm sunar.
Redis desteği ile token'ların kara listeye alınması ve refresh token'ların yönetimi gibi işlemleri kolayca gerçekleştirebilirsiniz.
Projenize entegre ederek güvenli ve esnek bir kimlik doğrulama mekanizması oluşturabilirsiniz.