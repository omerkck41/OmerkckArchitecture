---

# ApiHelperLibrary

`ApiHelperLibrary`, ASP.NET Core projelerinde tutarlı ve profesyonel API yanıtları oluşturmayı kolaylaştıran bir yardımcı kütüphanedir. Bu kütüphane, standart yanıt formatları, otomatik durum kodları ve dinamik `Location` header'ı gibi özellikler sunar.

## Özellikler

- **Standart Yanıt Formatı:** Tüm API yanıtları için tutarlı bir yapı (`ApiResponse<T>`).
- **Otomatik Durum Kodları:** HTTP metoduna göre otomatik olarak uygun durum kodları (200, 201, 400, 404, 204 vb.).
- **Dinamik Location Header:** POST isteklerinde otomatik olarak `Location` header'ı oluşturma.
- **Esnek ve Genişletilebilir:** Kolayca özelleştirilebilir ve genişletilebilir yapı.

## Kurulum

1. **Projeye Ekleme:**
   - Kütüphaneyi projenize referans olarak ekleyin.
   - `.csproj` dosyanıza aşağıdaki satırı ekleyin:

     ```xml
     <ItemGroup>
       <ProjectReference Include="..\ApiHelperLibrary\ApiHelperLibrary.csproj" />
     </ItemGroup>
     ```

2. **Startup'da Kütüphaneyi Kullanma:**
   - `Startup.cs` veya `Program.cs` dosyasında kütüphaneyi ekleyin:

     ```csharp
     public class Startup
     {
         public void ConfigureServices(IServiceCollection services)
         {
             services.AddApiHelperLibrary();
             services.AddControllers();
         }
     }
     ```

## Kullanım

### 1. Temel Controller Yapısı

Tüm controller'larınızı `ApiControllerBase` sınıfından türeterek kütüphanenin özelliklerini kullanabilirsiniz.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        var product = _productService.GetProductById(id);
        if (product == null)
        {
            var response = ApiResponseHelper.CreateFailResponse<object>("Ürün bulunamadı.", HttpContext);
            return HandleResult(response);
        }

        var successResponse = ApiResponseHelper.CreateSuccessResponse(product, "Ürün başarıyla getirildi.", HttpContext);
        return HandleResult(successResponse);
    }
}
```

### 2. Yanıt Oluşturma

`ApiResponseHelper` sınıfı ile başarılı veya başarısız yanıtlar oluşturabilirsiniz.

#### Başarılı Yanıt Örneği

```csharp
[HttpPost]
public IActionResult CreateProduct([FromBody] CreateProductRequest request)
{
    var productId = _productService.CreateProduct(request);
    var response = ApiResponseHelper.CreateSuccessResponse(
        new { Id = productId },
        "Ürün başarıyla oluşturuldu.",
        HttpContext,
        productId
    );
    return HandleResult(response);
}
```

#### Başarısız Yanıt Örneği

```csharp
[HttpPut("{id}")]
public IActionResult UpdateProduct(int id, [FromBody] UpdateProductRequest request)
{
    var product = _productService.GetProductById(id);
    if (product == null)
    {
        var response = ApiResponseHelper.CreateFailResponse<object>("Ürün bulunamadı.", HttpContext);
        return HandleResult(response);
    }

    _productService.UpdateProduct(id, request);
    var successResponse = ApiResponseHelper.CreateSuccessResponse<object>(null, "Ürün başarıyla güncellendi.", HttpContext);
    return HandleResult(successResponse);
}
```

### 3. Otomatik Durum Kodları

`ApiResponseHelper`, HTTP metoduna göre otomatik olarak uygun durum kodlarını belirler:

- **GET:** 200 OK, 404 Not Found, 204 No Content
- **POST:** 201 Created, 400 Bad Request
- **PUT:** 200 OK, 204 No Content, 404 Not Found
- **DELETE:** 204 No Content, 404 Not Found

### 4. Dinamik Location Header

POST isteklerinde, `Location` header'ı otomatik olarak oluşturulur:

```csharp
[HttpPost]
public IActionResult CreateProduct([FromBody] CreateProductRequest request)
{
    var productId = _productService.CreateProduct(request);
    var response = ApiResponseHelper.CreateSuccessResponse(
        new { Id = productId },
        "Ürün başarıyla oluşturuldu.",
        HttpContext,
        productId
    );
    return HandleResult(response);
}
```

## Proje Yapısı

```
ApiHelperLibrary/
│
├── Responses/                        // Yanıt yapıları
│   ├── ApiResponse.cs                // Temel yanıt sınıfı
│   └── ApiResponseExtensions.cs      // Yanıt sınıfı için extension metodlar
│
├── Helpers/                          // Yardımcı sınıflar
│   ├── ApiResponseHelper.cs          // Yanıt oluşturma yardımcısı
│   ├── LocationHelper.cs             // Location header oluşturma yardımcısı
│   └── HttpContextExtensions.cs      // HttpContext için extension metodlar
│
├── Controllers/                      // Temel controller yapısı
│   └── ApiControllerBase.cs          // Temel controller sınıfı
│
└── Extensions/                       // Middleware ve extension metodlar
    └── ServiceCollectionExtensions.cs // IServiceCollection için extension metodlar
```

## Katkıda Bulunma

Bu proje açık kaynaklıdır. Katkıda bulunmak için:

1. Repoyu forklayın.
2. Yeni bir branch oluşturun (`git checkout -b feature/AmazingFeature`).
3. Değişikliklerinizi commit edin (`git commit -m 'Add some AmazingFeature'`).
4. Branch'i pushlayın (`git push origin feature/AmazingFeature`).
5. Pull Request açın.

## Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Daha fazla bilgi için `LICENSE` dosyasına bakın.

---