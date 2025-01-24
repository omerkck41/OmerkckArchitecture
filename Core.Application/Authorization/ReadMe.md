---

# Core.Application.Authorization Kütüphanesi

Bu kütüphane, **rol tabanlı** ve **claim tabanlı** yetkilendirme işlemlerini kolayca yönetmek için geliştirilmiştir. Büyük projelerde kullanılabilecek şekilde tasarlanmış olup, **MediatR pipeline'ı**, **middleware** ve **policy-based authorization** gibi modern yaklaşımları destekler.

---

## **Neden Bu Kütüphane Kullanılmalı?**

- **Modüler Yapı**: Rol ve claim tabanlı yetkilendirme işlemlerini merkezi bir şekilde yönetir.
- **Esneklik**: Hem MediatR pipeline'ı hem de HTTP middleware'ı ile entegre çalışabilir.
- **Policy-Based Authorization**: Yetkilendirme kurallarını policy'ler üzerinden tanımlayarak, daha esnek ve yönetilebilir bir yapı sunar.
- **Büyük Projeler İçin Uygun**: Performans optimizasyonu ve genişletilebilirlik özellikleriyle büyük projelerde rahatlıkla kullanılabilir.

---

## **Kütüphanenin Temel Özellikleri**

1. **Rol Tabanlı Yetkilendirme**:
   - Kullanıcının belirli rollere sahip olup olmadığını kontrol eder.
   - Örneğin: `Admin`, `Manager`, `User` gibi roller.

2. **Claim Tabanlı Yetkilendirme**:
   - Kullanıcının belirli claim'lere sahip olup olmadığını kontrol eder.
   - Örneğin: `Permission:ViewOrders`, `Permission:ManageOrders` gibi claim'ler.

3. **Policy-Based Authorization**:
   - Yetkilendirme kurallarını policy'ler üzerinden tanımlar.
   - Örneğin: `ViewOrdersPolicy`, `ManageOrdersPolicy`.

4. **MediatR Pipeline Entegrasyonu**:
   - MediatR request'leri öncesinde otomatik yetkilendirme kontrolü yapar.

5. **HTTP Middleware**:
   - HTTP istekleri sırasında yetkilendirme kontrolü yapar.

---

## **Kurulum**

Kütüphaneyi projenize eklemek için aşağıdaki adımları izleyin:

1. **Service Collection'a Ekleme**:
   - `Startup.cs` veya `Program.cs` dosyasında gerekli servisleri ekleyin.
   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       services.AddAuthorizationPipeline();
       services.AddAuthorization(options =>
       {
           AuthorizationPolicies.ConfigurePolicies(options);
       });
   }
   ```

2. **Middleware'ı Ekleyin**:
   - HTTP pipeline'ına yetkilendirme middleware'ını ekleyin.
   ```csharp
   public void Configure(IApplicationBuilder app)
   {
       app.UseHttpAuthorizationMiddleware();
   }
   ```

---

## **Kullanım Örnekleri**

### 1. **Policy-Based Authorization**

Policy'ler üzerinden yetkilendirme kurallarını tanımlayabilir ve bu policy'leri controller veya endpoint'lerde kullanabilirsiniz.

```csharp
[Authorize(Policy = AuthorizationPolicies.ViewOrders)]
public IActionResult GetOrders()
{
    // Orders logic
}
```

### 2. **MediatR Pipeline ile Yetkilendirme**

MediatR request'leri öncesinde otomatik yetkilendirme kontrolü yapabilirsiniz.

```csharp
public class GetOrdersQuery : IRequest<List<string>>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Admin, GeneralOperationClaims.Manager];
    public Dictionary<string, string> Claims => new() { { "Permission", "ViewOrders" } };
}
```

### 3. **HTTP Middleware ile Yetkilendirme**

HTTP istekleri sırasında yetkilendirme kontrolü yapabilirsiniz.

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseHttpAuthorizationMiddleware();
}
```

### 4. **Manuel Yetkilendirme Kontrolü**

`AuthorizationService` sınıfını kullanarak manuel yetkilendirme kontrolü yapabilirsiniz.

```csharp
public class OrdersController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    public OrdersController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public IActionResult GetOrders()
    {
        var user = HttpContext.User;
        var requiredRoles = new[] { GeneralOperationClaims.Admin, GeneralOperationClaims.Manager };
        var requiredClaims = new Dictionary<string, string> { { "Permission", "ViewOrders" } };

        _authorizationService.Authorize(user, requiredRoles, requiredClaims);

        // Orders logic
    }
}
```

---

## **Genişletilebilirlik**

Kütüphaneyi genişletmek için aşağıdaki adımları izleyebilirsiniz:

1. **Yeni Policy'ler Ekleyin**:
   - `AuthorizationPolicies` sınıfına yeni policy'ler ekleyebilirsiniz.
   ```csharp
   public static void ConfigurePolicies(AuthorizationOptions options)
   {
       options.AddPolicy("DeleteOrdersPolicy", policy =>
       {
           policy.RequireAuthenticatedUser();
           policy.RequireRole(GeneralOperationClaims.Admin);
           policy.RequireClaim("Permission", "DeleteOrders");
       });
   }
   ```

2. **Özel Yetkilendirme Kuralları**:
   - `AuthorizationService` sınıfını genişleterek, özel yetkilendirme kuralları ekleyebilirsiniz.

---

## **Test Etme**

Kütüphaneyi test etmek için unit test ve integration testler yazabilirsiniz. Özellikle `AuthorizationBehavior` ve `AuthorizationService` sınıfları için testler yazılmalıdır.

```csharp
[Fact]
public void Authorize_ShouldThrowException_WhenUserIsNotAuthenticated()
{
    var user = new ClaimsPrincipal();
    var requiredRoles = new[] { GeneralOperationClaims.Admin };
    var requiredClaims = new Dictionary<string, string>();

    var authorizationService = new AuthorizationService();

    Assert.Throws<AuthorizationException>(() =>
    {
        authorizationService.Authorize(user, requiredRoles, requiredClaims);
    });
}
```

---

## **Katkıda Bulunma**

Bu kütüphaneye katkıda bulunmak isterseniz, lütfen bir **pull request** gönderin. Katkılarınızı bekliyoruz!

---

## **Lisans**

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.

---

Bu **README.md** dosyası, kütüphanenizin ne işe yaradığını, nasıl kullanılacağını ve genişletilebileceğini açıklayan detaylı bir rehber sunar. Bu şablonu projenize uygun şekilde özelleştirebilirsiniz.