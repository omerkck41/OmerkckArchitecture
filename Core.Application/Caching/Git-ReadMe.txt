# Core.Application.Caching

Core.Application.Caching, modern yazılım projelerinde cache yönetimi için esnek, performanslı ve geliştirilebilir bir yapı sunar.
Bu sistem iki ana kullanım yöntemini destekler:
1. **Behavior Tabanlı Kullanım**: MediatR pipeline entegrasyonu.
2. **Service Tabanlı Kullanım**: Manuel cache yönetimi.

## Şema
```
Core.Application
└── Caching
    ├── Behavior
    │   ├── CachingBehavior.cs
    │   ├── CacheRemovingBehavior.cs
    │   ├── ICachableRequest.cs
    │   └── ICacheRemoverRequest.cs
    ├── Services
    │   ├── ICacheService.cs
    │   ├── InMemoryCacheService.cs
    │   └── DistributedCacheService.cs
    ├── CacheExtensions.cs
    ├── CacheKeyHelper.cs
    └── CacheSettings.cs
```

---

## Bileşenler ve Kullanım

### 1. Behavior Tabanlı Kullanım
Behavior yapısı, MediatR ile pipeline entegrasyonu sağlar. Cache ekleme veya temizleme otomatik olarak yönetilir.

#### 1.1 CachingBehavior
**CachingBehavior**, istek yanıtlandıktan sonra yanıtı cache'e ekler.

- **ICachableRequest**: Cache kullanımı için isteklerin implement etmesi gereken arayüz.

```csharp
public class GetUserQuery : IRequest<User>, ICachableRequest
{
    public int UserId { get; set; }
    public bool UseCache => true;
    public string CacheKey => CacheKeyHelper.GenerateKey("GetUser", UserId.ToString());
    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(10);
}
```

**CachingBehavior.cs**:
```csharp
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachableRequest
{
    private readonly ICacheService _cacheService;

    public CachingBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!request.UseCache)
            return await next();

        var cacheKey = request.CacheKey;
        if (await _cacheService.ExistsAsync(cacheKey, cancellationToken))
            return await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

        var response = await next();
        await _cacheService.SetAsync(cacheKey, response, request.CacheExpiration ?? TimeSpan.FromMinutes(30), cancellationToken);
        return response;
    }
}
```

#### 1.2 CacheRemovingBehavior
**CacheRemovingBehavior**, istek tamamlandığında ilgili cache'i temizler.

- **ICacheRemoverRequest**: Cache temizleme için isteklerin implement etmesi gereken arayüz.

```csharp
public class DeleteUserCommand : IRequest, ICacheRemoverRequest
{
    public int UserId { get; set; }
    public string CacheKey => CacheKeyHelper.GenerateKey("GetUser", UserId.ToString());
}
```

**CacheRemovingBehavior.cs**:
```csharp
public class CacheRemovingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheRemoverRequest
{
    private readonly ICacheService _cacheService;

    public CacheRemovingBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
        if (!string.IsNullOrEmpty(request.CacheKey))
            await _cacheService.RemoveAsync(request.CacheKey, cancellationToken);

        return response;
    }
}
```

---

### 2. Service Tabanlı Kullanım
Service tabanlı yaklaşım, MediatR olmadan manuel cache yönetimi sunar.

#### ICacheService
ICacheService, cache operasyonlarını soyutlayan temel arayüzdür.
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
```

#### Kullanım Örneği:

```csharp
public class UserService
{
    private readonly ICacheService _cacheService;

    public UserService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<User> GetUserAsync(int id)
    {
        string cacheKey = CacheKeyHelper.GenerateKey("User", id.ToString());

        if (await _cacheService.ExistsAsync(cacheKey))
            return await _cacheService.GetAsync<User>(cacheKey);

        var user = new User { Id = id, Name = "John Doe" };
        await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(10));

        return user;
    }
}
```

---

### 3. Proje Entegrasyonu
**CacheExtensions**, Dependency Injection ile cache servislerini projenize ekler.

#### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Cache ayarları
var cacheSettings = new CacheSettings { Provider = "InMemory", DefaultExpiration = TimeSpan.FromMinutes(30) };
builder.Services.AddCacheServices(cacheSettings);

// MediatR Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheRemovingBehavior<,>));

var app = builder.Build();
app.Run();
```

---

### 4. Yardımcı Sınıflar

#### CacheKeyHelper
Cache anahtarlarını standartlaştırmak için kullanılır.
```csharp
public static string GenerateKey(params string[] parts)
{
    return string.Join(":", parts);
}
```

#### CacheSettings
Cache sağlayıcılarının ve varsayılan ayarların tanımlanması.
```csharp
public class CacheSettings
{
    public string Provider { get; set; } = "InMemory";
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);
}
```

---

## Sonuç
Bu yapı hem MediatR pipeline üzerinden hem de manuel servis bazlı olarak esnek bir cache yönetimi sunar.
Projelerinizde kolayca entegre ederek performansı artırabilir ve dinamik cache yönetimi sağlayabilirsiniz.

