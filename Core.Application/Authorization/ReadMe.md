---

# Core.Application.Authorization Kütüphanesi Yetkilendirme Modülü

Bu modül, ASP.NET Core uygulamalarınızda güvenli, modüler ve ölçeklenebilir yetkilendirme işlemleri gerçekleştirmek amacıyla geliştirilmiştir. Hem MediatR pipeline üzerinden hem de ASP.NET Core’un policy tabanlı yetkilendirme mekanizması ile entegrasyon sağlanacak şekilde tasarlanmıştır.

## İçerik

- [Genel Bakış](#genel-bakış)
- [Özellikler](#özellikler)
- [Kurulum](#kurulum)
- [Kullanım Senaryoları](#kullanım-senaryoları)
  - [MediatR Pipeline Üzerinde Yetkilendirme](#mediatr-pipeline-üzerinde-yetkilendirme)
  - [ASP.NET Core Policy Tabanlı Yetkilendirme](#aspnet-core-policy-tabanlı-yetkilendirme)
- [Hata Yönetimi ve Loglama](#hata-yönetimi-ve-loglama)

## Genel Bakış

Bu modül, kullanıcıların roller ve claim’ler üzerinden yetkilendirilmesini sağlamak için aşağıdaki prensiplere dayanmaktadır:
- **Modülerlik:** Rol ve claim kontrolleri ayrı metotlar ve sınıflar halinde organize edilmiştir.
- **Genişletilebilirlik:** Yeni roller, claim’ler ve politikalar eklenebilmesi için esnek bir yapı sunar.
- **Modern Yaklaşım:** .NET 9.0 ve güncel C# özellikleri (target-typed new, minimal API'ler vs.) kullanılarak yazılmıştır.

## Özellikler

- **Rol ve Claim Kontrolleri:** Kullanıcıların sahip olduğu roller ve claim’ler, ayrı metotlarla kontrol edilmekte ve “Admin” veya “Manager” gibi özel roller için bypass mekanizması sunulmaktadır.
- **MediatR Pipeline Entegrasyonu:** Yetkilendirme, MediatR isteklerinin işlenmesi sırasında otomatik olarak devreye girer.
- **Policy Tabanlı Yetkilendirme:** ASP.NET Core AuthorizationPolicy ile esnek politika yapılandırması yapılır.
- **Özel Middleware:** HTTP isteklerinde kimlik doğrulama kontrolünü sağlayan middleware yer almaktadır.
- **Unit Test Desteği:** Soyutlanmış servis yapısı sayesinde bileşenlerin unit testleri kolayca yazılabilir.

---


## Kurulum

1. **Proje Entegrasyonu:**  
   Yetkilendirme modülünü, projenize dahil edin. Modül, .NET 9.0 uyumlu olacak şekilde güncellenmiştir.

2. **Bağımlılıkların Yüklenmesi:**  
   Projede aşağıdaki NuGet paketlerine ihtiyaç duyulmaktadır:
   - `Microsoft.AspNetCore.Http`
   - `MediatR`
   - `Microsoft.Extensions.Logging`

3. **Startup (Program.cs) Ayarları:**  
   Aşağıda minimal API kullanılarak modül entegrasyonuna ilişkin örnek yapılandırma yer almaktadır:

   ```csharp
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.Extensions.DependencyInjection;
   using Core.Application.Authorization.Services;
   using Core.Application.Authorization.Behaviors;
   using Core.Application.Authorization.Middleware;

   var builder = WebApplication.CreateBuilder(args);

   // Servis eklemeleri
   builder.Services.AddHttpContextAccessor();
   builder.Services.AddLogging();

   // MediatR entegrasyonu ve AuthorizationBehavior eklenmesi
   builder.Services.AddMediatR(typeof(AuthorizationBehavior<,>).Assembly);
   builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

   // Policy'leri yapılandırın
   builder.Services.AddAuthorization(options => {
       AuthorizationPolicies.ConfigurePolicies(options);
   });

   var app = builder.Build();

   // Custom HTTP Authorization Middleware ekleniyor
   app.UseMiddleware<HttpAuthorizationMiddleware>();

   // Minimal API örneği: Yetkilendirilmiş endpoint
   app.MapGet("/secure-data", [Authorize(Policy = AuthorizationPolicies.ViewDataPolicy)] () => {
       return Results.Ok("Güvenli veri erişildi.");
   });

   app.Run();
   ```

## Kullanım Senaryoları
### MediatR Pipeline Üzerinde Yetkilendirme
### Örnek İstek Sınıfı (Query):

```csharp
using MediatR;
using Core.Application.Authorization.Models;

public class GetSecureDataQuery : IRequest<string>, ISecuredRequest
{
    // Bu istek için gerekli roller ve claim'ler
    public string[] Roles => new[] { GeneralOperationClaims.User };
    public Dictionary<string, string> Claims => new Dictionary<string, string>
    {
        { "Permission", "ViewData" }
    };
}
```

### İstek İşleyici (Handler):

```csharp
using MediatR;

public class GetSecureDataQueryHandler : IRequestHandler<GetSecureDataQuery, string>
{
    public Task<string> Handle(GetSecureDataQuery request, CancellationToken cancellationToken)
    {
        // MediatR pipeline'ı içerisindeki AuthorizationBehavior, 
        // burada gerekli yetkilendirme kontrollerini otomatik olarak yapacaktır.
        return Task.FromResult("Güvenli veri işleniyor.");
    }
}
```
---
## ASP.NET Core Policy Tabanlı Yetkilendirme
Endpoint'lerde, [Authorize(Policy = "PolicyName")] attribute'u kullanılarak kullanıcıların erişim yetkileri denetlenir. Yukarıdaki örnekte, /secure-data endpoint’i ViewDataPolicy ile korunmuştur. Bu politika, yalnızca "ViewData" iznine sahip Admin veya Manager rollerine erişim sağlar.

## Hata Yönetimi ve Loglama
AuthorizationException:
Yetkilendirme kontrollerinde bir hata meydana geldiğinde, AuthorizationException fırlatılır. Bu sayede, hangi kontrollerin başarısız olduğu detaylıca loglanır.


## Politika Yapılandırması:
AuthorizationPolicies sınıfı, yeni roller ve claim'ler eklenerek genişletilebilir. Ek parametrelerle daha esnek yapılandırmalar oluşturabilirsiniz.

## Unit Test Desteği:
Tüm bileşenler soyutlanmış arayüzler ve modüler yapılar sayesinde unit test yazımına uygundur. Bu sayede, her bileşenin izole bir şekilde test edilmesi kolaylaşır.

---