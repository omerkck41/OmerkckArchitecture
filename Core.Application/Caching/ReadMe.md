Aşağıda, projeniz için detaylı bir `README.md` dosyası örneği bulacaksınız. Bu dosya, projenin ne yaptığını, hangi teknolojilerin kullanıldığını, nasıl entegre edileceğini ve kullanım örneklerini içerir.

---

# Core Caching Library

Bu proje, .NET 9.0 mimarisinde geliştirilmiş bir caching (önbellekleme) kütüphanesidir. Büyük ölçekli projelerde kullanılabilecek şekilde tasarlanmıştır ve **In-Memory** ve **Distributed** cache yöntemlerini destekler. MediatR pipeline'ına entegre edilerek, cache işlemlerini otomatikleştirir.

## Özellikler

- **In-Memory Cache**: Bellek içi önbellekleme için `IMemoryCache` kullanır.
- **Distributed Cache**: Dağıtık önbellekleme için `IDistributedCache` kullanır (Redis, SQL Server, vs.).
- **MediatR Pipeline Entegrasyonu**: Cache işlemleri otomatik olarak MediatR pipeline'ına entegre edilir.
- **Esnek Yapı**: Cache anahtarı oluşturma, cache süresi belirleme ve cache temizleme işlemleri kolayca yönetilebilir.
- **Best Practices**: Clean Code ve SOLID prensiplerine uygun olarak geliştirilmiştir.

## Kullanılan Teknolojiler

- **.NET 9.0**: Proje .NET 9.0 mimarisinde geliştirilmiştir.
- **MediatR**: CQRS pattern'ini uygulamak ve pipeline davranışlarını yönetmek için kullanılır.
- **Microsoft.Extensions.Caching.Memory**: In-Memory cache işlemleri için kullanılır.
- **Microsoft.Extensions.Caching.Distributed**: Distributed cache işlemleri için kullanılır.
- **System.Text.Json**: JSON serileştirme ve deserileştirme işlemleri için kullanılır.

## Projeye Entegrasyon

### 1. **Program.cs Ayarları**

Projeye caching servislerini eklemek için `Program.cs` dosyasında aşağıdaki ayarları yapın:

```csharp
using Core.Application.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Cache ayarlarını yapılandır
builder.Services.AddCachingServices(settings =>
{
    settings.Provider = CacheProvider.InMemory; // Veya CacheProvider.Distributed
    settings.DefaultExpiration = TimeSpan.FromMinutes(30);
});

// Distributed cache için Redis kullanılacaksa:
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("Redis");
// });

var app = builder.Build();

app.Run();
```

### 2. **appsettings.json Ayarları**

`appsettings.json` dosyasında cache ayarlarını yapılandırabilirsiniz:

```json
{
  "CacheSettings": {
    "Provider": "InMemory", // "Distributed" olarak da ayarlanabilir
    "DefaultExpiration": "00:30:00" // 30 dakika
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379" // Distributed cache için Redis bağlantı dizesi
  }
}
```

### 3. **Cache Key Oluşturma**

Cache anahtarı oluşturmak için `CacheKeyHelper` sınıfını kullanabilirsiniz:

```csharp
var cacheKey = CacheKeyHelper.GenerateKey("user", userId.ToString());
```

### 4. **MediatR Pipeline Entegrasyonu**

Cache işlemleri otomatik olarak MediatR pipeline'ına entegre edilir. Örneğin, bir query için cache kullanmak istiyorsanız:

```csharp
public class GetUserQuery : IRequest<User>, ICachableRequest
{
    public int UserId { get; set; }

    public bool UseCache => true;
    public string CacheKey => CacheKeyHelper.GenerateKey("user", UserId.ToString());
    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(10);
}
```

### 5. **Cache Temizleme**

Cache temizleme işlemi için `ICacheRemoverRequest` arayüzünü uygulayın:

```csharp
public class UpdateUserCommand : IRequest, ICacheRemoverRequest
{
    public int UserId { get; set; }

    public string CacheKey => CacheKeyHelper.GenerateKey("user", UserId.ToString());
    public bool RemoveCache => true;
}
```

## Kullanım Örnekleri

### 1. **In-Memory Cache Kullanımı**

```csharp
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Veritabanından kullanıcıyı getir
        return await _userRepository.GetUserByIdAsync(request.UserId);
    }
}
```

### 2. **Distributed Cache (Redis) Kullanımı**

```csharp
public class GetProductQuery : IRequest<Product>, ICachableRequest
{
    public int ProductId { get; set; }

    public bool UseCache => true;
    public string CacheKey => CacheKeyHelper.GenerateKey("product", ProductId.ToString());
    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(15);
}

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Product>
{
    private readonly IProductRepository _productRepository;

    public GetProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Product> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        // Veritabanından ürünü getir
        return await _productRepository.GetProductByIdAsync(request.ProductId);
    }
}
```

### 3. **Cache Temizleme Örneği**

```csharp
public class UpdateProductCommand : IRequest, ICacheRemoverRequest
{
    public int ProductId { get; set; }
    public string Name { get; set; }

    public string CacheKey => CacheKeyHelper.GenerateKey("product", ProductId.ToString());
    public bool RemoveCache => true;
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Ürünü güncelle
        await _productRepository.UpdateProductAsync(request.ProductId, request.Name);
    }
}
```

## Faydalar

- **Performans Artışı**: Sık erişilen veriler önbelleğe alınarak, veritabanı sorguları azaltılır ve uygulama performansı artar.
- **Esneklik**: Hem In-Memory hem de Distributed cache yöntemleri desteklenir.
- **Otomasyon**: MediatR pipeline'ına entegre edilerek, cache işlemleri otomatikleştirilir.
- **Kolay Entegrasyon**: Basit yapılandırma ve kullanım örnekleri ile kolayca projeye entegre edilebilir.

---