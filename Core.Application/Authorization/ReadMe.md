# Yetkilendirme Sistemi (Authorization Behavior ve Service)

Bu doküman, MediatR pipeline için tasarlanan **AuthorizationBehavior** ve bağımsız bir servis olarak kullanılabilen **AuthorizationService** yapısını tanıtır. Yetkilendirme süreçlerini nasıl yapılandıracağınızı, projeye nasıl entegre edeceğinizi ve örnek kullanım senaryolarını açıklar.

---

## Teknolojiler ve Kullanılan Yapılar

- **.NET 9.0**: En güncel framework sürümü.
- **MediatR**: CQRS ve pipeline tabanlı istek/yanıt modelini uygulamak için.
- **ASP.NET Core**: HTTP tabanlı middleware entegrasyonu için.
- **Dependency Injection**: Modüler ve test edilebilir bir yapı sağlamak için.

---

## Yapının Genel Tanımı

### 1. **AuthorizationBehavior**
MediatR pipeline içerisinde çalışarak isteklerin yetkilendirme kontrolünü otomatik olarak yapar. Role ve claim bazlı yetkilendirme desteklenir.

### 2. **AuthorizationService**
Yetkilendirme işlemlerini bağımsız bir servis olarak sunar. API endpoint'leri veya başka iş akışlarında manuel olarak kullanılabilir.

---

## Yapıyı Projeye Ekleme

### 1. Gereksinimler
- **.NET 9.0 SDK**
- `MediatR` ve `MediatR.Extensions.Microsoft.DependencyInjection` NuGet paketleri

### 2. NuGet Paketlerini Yükleyin

```bash
Install-Package MediatR
Install-Package MediatR.Extensions.Microsoft.DependencyInjection
```

### 3. Servisleri Kaydetme
`Program.cs` veya `Startup.cs` dosyanıza aşağıdaki kodu ekleyin:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Authorization Pipeline ve Service Kaydı
builder.Services.AddAuthorizationPipeline();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

var app = builder.Build();

app.UseAuthorizationMiddleware();
app.MapControllers();

app.Run();
```

---

## Ayarlar

### GeneralOperationClaims
Rolleri ve genel yetkilendirme ayarlarını yönetmek için kullanılan statik sınıf:

```csharp
public static class GeneralOperationClaims
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
}
```

Bu sınıf, yetkilendirme gereksinimlerinize göre genişletilebilir.

---

## Kullanım

### 1. **AuthorizationBehavior** Kullanımı
MediatR pipeline'da otomatik yetkilendirme kontrolü yapmak için:

#### Örnek İstek

```csharp
public class GetOrdersQuery : IRequest<List<string>>, ISecuredRequest
{
    public string[] Roles => new[] { GeneralOperationClaims.Admin, GeneralOperationClaims.Manager };
    public Dictionary<string, string> Claims => new Dictionary<string, string> { { "Permission", "ViewOrders" } };
}
```

Yetkilendirme kontrolü otomatik olarak yapılır. Roller ve claim'ler isteğe özel olarak belirtilir.

### 2. **AuthorizationService** Kullanımı
Bağımsız bir servis olarak manuel yetkilendirme yapmak için:

#### Örnek Kullanım

```csharp
[HttpGet]
public IActionResult GetOrders([FromServices] IAuthorizationService authorizationService)
{
    var user = HttpContext.User;

    // Gerekli roller ve claim'ler
    var requiredRoles = new[] { GeneralOperationClaims.Manager };
    var requiredClaims = new Dictionary<string, string> { { "Permission", "ViewOrders" } };

    // Yetkilendirme kontrolü
    authorizationService.Authorize(user, requiredRoles, requiredClaims);

    return Ok(new { Orders = "List of orders" });
}
```

---

## Özet

Bu yapı, hem MediatR pipeline ile otomatik yetkilendirme kontrolü yapmak hem de bağımsız bir servis olarak manuel yetkilendirme sağlamak için tasarlanmıştır.
Modüler, esnek ve genişletilebilir bir çözüm sunar.