# Authorization Module - Kullanım Rehberi

## **1. Authorization Modülüne Genel Bakış**

Authorization modülü, kullanıcının rollerine veya taleplerine (claims) dayalı olarak erişim kontrolü sağlar. Bu yapı hem **Role-Based Authorization** hem de **Policy-Based Authorization** yeteneklerini içerir ve esnek, genişletilebilir bir mimari sunar.

### **Kullanılan Teknolojiler**
- **Microsoft.AspNetCore.Authorization**: ASP.NET Core'un yerleşik yetkilendirme altyapısı.
- **Dependency Injection (DI)**: Yetkilendirme kurallarını merkezi bir şekilde tanımlamak ve uygulamak için kullanılır.
- **Clean Code ve Best Practices**: Modüler, okunabilir ve test edilebilir bir yapı sağlar.

### **Desteklenen Yetkilendirme Türleri**
1. **Role-Based Authorization**: Kullanıcının belirli bir rol (örneğin "Admin") içerisinde olup olmadığını kontrol eder.
2. **Policy-Based Authorization**: Kullanıcının belirli bir talebi (örneğin "CustomClaim") sağlayıp sağlamadığını kontrol eder.

---

## **2. Modülü Başka Bir Projede Kullanma**

### **Adım 1: Gerekli Paketlerin Yüklenmesi**
Projenize aşağıdaki NuGet paketlerini ekleyin:

```bash
Install-Package Microsoft.AspNetCore.Authorization
Install-Package Microsoft.AspNetCore.Authentication
Install-Package Microsoft.AspNetCore.Authorization.Policy
```

### **Adım 2: Dependency Injection ile Yapılandırma**
`Program.cs` dosyasında Authorization modülünü yapılandırın:

```csharp
using Core.Application.Authorization.Behaviors;
using Core.Application.Authorization.Services;

var builder = WebApplication.CreateBuilder(args);

// Role-Based Policies
AuthorizationPolicyService.AddRolePolicies(builder.Services);

// Claim-Based Policies
AuthorizationPolicyService.AddClaimPolicies(builder.Services);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

### **Adım 3: Policy Tanımları**
Policy'lerin `AuthorizationPolicyService` içerisinde merkezi olarak tanımlandığından emin olun:

```csharp
public static class AuthorizationPolicyService
{
    public static void AddRolePolicies(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy =>
                policy.Requirements.Add(new RolesRequirement(new[] { "Admin" })));
        });

        services.AddSingleton<IAuthorizationHandler, RoleAuthorizationBehavior>();
    }

    public static void AddClaimPolicies(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("CustomClaimPolicy", policy =>
                policy.Requirements.Add(new ClaimRequirement("CustomClaim", "Allowed")));
        });

        services.AddSingleton<IAuthorizationHandler, ClaimAuthorizationBehavior>();
    }
}
```

### **Adım 4: Controller veya Endpoint Kullanımı**
Controller seviyesinde yetkilendirme politikalarını şu şekilde uygulayabilirsiniz:

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = "AdminPolicy")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetAdminDashboard()
    {
        return Ok("Access granted for Admins");
    }
}

[Authorize(Policy = "CustomClaimPolicy")]
[ApiController]
[Route("api/[controller]")]
public class CustomClaimController : ControllerBase
{
    [HttpGet("custom-claim-data")]
    public IActionResult GetCustomClaimData()
    {
        return Ok("Access granted for users with CustomClaim");
    }
}
```

---

## **3. Avantajları ve Faydaları**

### **3.1. Modüler ve Esnek Mimari**
- Yeni roller veya talepler kolayca tanımlanabilir.
- Yetkilendirme kuralları merkezi olarak yönetilir ve değiştirilebilir.

### **3.2. Temiz Kod ve Test Edilebilirlik**
- Yetkilendirme kuralları ayrı katmanlarda tanımlandığı için test edilebilir.
- İş mantığı ve yetkilendirme kodları birbirinden ayrılır.

### **3.3. Güvenlik ve Kontrol**
- Hassas verilere yalnızca yetkili kullanıcıların erişimini sağlar.
- Kullanıcı bazlı veya talep bazlı erişim kontrolü sağlar.

---

## **4. Örnek Senaryo: Kullanıcı Rolleri ile Yetkilendirme**
Bir "E-Ticaret Yönetim Paneli" için yetkilendirme:

### Kullanıcı Rolleri:
- **Admin**: Siparişleri ve kullanıcı bilgilerini görüntüleyebilir.
- **Manager**: Yalnızca sipariş bilgilerini görüntüleyebilir.

### Adım 1: Role Policies Tanımlama
```csharp
AuthorizationPolicyService.AddRolePolicies(builder.Services);
```
### Adım 2: Controller Seviyesi Yetkilendirme
```csharp
[Authorize(Policy = "AdminPolicy")]
public IActionResult ViewOrders()
{
    return Ok("Orders displayed for Admin.");
}

[Authorize(Policy = "ManagerPolicy")]
public IActionResult ViewBasicOrders()
{
    return Ok("Basic orders displayed for Manager.");
}
```