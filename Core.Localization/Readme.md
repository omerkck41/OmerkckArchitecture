# Core.Localization

Core.Localization, ASP.NET Core projelerinde çoklu dil, para birimi ve tarih/saat formatlamayı kolaylaştıran, modüler ve genişletilebilir bir kütüphanedir. Hem JSON dosyalarından hem de `.resx` kaynaklarından çeviri alabilir, MediatR pipeline davranışıyla entegre olur, MVC veya API katmanında sorunsuz çalışır.

---

## 1) Nedir?

- **Kütüphane Amacı:** Uygulamalarınızı birden fazla dile ve kültüre kolayca adapte etmenizi sağlar.
- **Özellikler:**
  - Anahtar/değer çeviri desteği (JSON dosyaları, `.resx` kaynakları)
  - CultureInfo tabanlı tarih/saat ve para birimi formatlama
  - Thread-safe kültür yönetimi (`AsyncLocal<CultureInfo>`)
  - Cache desteği (`IDistributedCache` ile bellek içi veya Redis)
  - MediatR Pipeline Behavior ile otomatik kültür set etme
  - MVC Controller/View için Action Filter

---

## 2) Nasıl Çalışır? Hangi Teknolojileri Kullanır?

1. **.NET & ASP.NET Core**
   - `IOptionsMonitor<LocalizationOptions>` ile dinamik konfigürasyon
   - `IDistributedCache` ile önbellekleme (Memory, Redis vb.)
   - `AsyncLocal<CultureInfo>` ile her async iş parçacığına özgü kültür
2. **MediatR Entegrasyonu**
   - `IPipelineBehavior<TRequest,TResponse>` kullanarak her istekten önce `ICultureProvider` ile kültürü alır ve `LocalizationServiceAsync`’e set eder.
3. **MVC Entegrasyonu**
   - `IActionFilter` (CultureActionFilter) ile her controller çağrısında kültürü set eder.
4. **Kaynak Sağlayıcılar**
   - `ResourceFileLocalizationSourceAsync` → `.resx` derleme zamanlı kaynak
   - `JsonFileLocalizationSourceAsync` → JSON dosyalarındaki çeviriler
   - İleride eklenecek DB/API kaynakları modüler şekilde eklenebilir

---

## 3) Projeye Ekleme ve Konfigürasyon

### A) GitHub Üzerinden Ekleme

1. **NuGet Paketi (önerilen)**
   ```bash
   dotnet add package Core.Localization
   ```
2. **Kaynak Kod Olarak**
   ```bash
   git clone https://github.com/omerkck41/OmerkckArchitecture.git
   cd OmerkckArchitecture/Core.Localization
   dotnet add reference ../YourProject/YourProject.csproj
   ```

### B) `appsettings.json` Ayarları
```json
"Localization": {
  "DefaultCulture": "tr-TR",
  "SupportedCultures": ["tr-TR","en-US","de-DE"],
  "ReturnKeyIfNotFound": true,
  "EnableCaching": true,
  "CacheTtlSeconds": 3600,
  "Currency": {
    "DefaultCurrency": "TRY",
    "ExchangeRateApiUrl": "",
    "ExchangeRateApiKey": "",
    "UpdateInterval": 60,
    "EnableCaching": true
  }
}
```

### C) DI ve Middleware Kayıtları (Program.cs)
```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Localization kütüphanesini ekle
builder.Services.AddCoreLocalization(builder.Configuration);

// 2. Cache ekleme (Memory veya Redis)
builder.Services.AddDistributedMemoryCache();
// veya
// builder.Services.AddStackExchangeRedisCache(opts => { ... });

// 3. MVC & MediatR
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<CultureActionFilter>();
});
builder.Services.AddMediatR(typeof(Program));

var app = builder.Build();

// 4. RequestLocalization middleware
temp var supported = builder.Configuration
    .GetSection("Localization:SupportedCultures").Get<string[]>();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supported.Select(c=>new CultureInfo(c)).ToList(),
    SupportedUICultures = supported.Select(c=>new CultureInfo(c)).ToList()
});

app.MapControllers();
app.Run();
```

---

## 4) Kullanım Örnekleri

### A) Backend (MediatR / API)

1. **Pipeline Behavior**
   ```csharp
   public class SampleRequest : IRequest<SampleResponse> { /* ... */ }

   public class SampleResponse : ILocalizedResponse
   {
       public string? MessageKey { get; set; }
       public string? Message { get; set; }
   }

   public class SampleHandler : IRequestHandler<SampleRequest,SampleResponse>
   {
       public Task<SampleResponse> Handle(SampleRequest req, CancellationToken ct)
       {
           return Task.FromResult(new SampleResponse { MessageKey = "WelcomeUser" });
       }
   }
   ```
   - Pipeline behavior, `MessageKey`’i alır, `LocalizationServiceAsync` ile çeviriyi getirir ve `Message` alanına koyar.

2. **Doğrudan Servis Kullanımı**
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class GreetController : ControllerBase
   {
       private readonly ILocalizationServiceAsync _loc;
       public GreetController(ILocalizationServiceAsync loc) => _loc = loc;

       [HttpGet]
       public async Task<IActionResult> Get()
       {
           var text = await _loc.GetStringAsync("Hello");
           return Ok(new { Text = text });
       }
   }
   ```

### B) Frontend (MVC / Razor)

1. **View’den Çeviri**
   ```cshtml
   @inject Core.Localization.Abstract.ILocalizationServiceAsync Loc
   <h1>@(await Loc.GetStringAsync("WelcomeUser", User.Identity.Name))</h1>
   ```
2. **Dropdown ile Dil Seçimi**
   ```cshtml
   <form method="get">
     <select name="culture" onchange="this.form.submit()">
       @foreach (var c in ViewBag.SupportedCultures as List<string>)
       {
           <option value="@c" selected="@(c == CultureInfo.CurrentCulture.Name)">@c</option>
       }
     </select>
   </form>
   ```
   - `HttpContextCultureProvider` query-string veya cookie’den kültürü alır.

3. **Para Birimi Formatlama**
   ```csharp
   @inject ICurrencyServiceAsync CurrencySvc
   @{
       var price = await CurrencySvc.FormatCurrencyAsync(1234.56m);
   }
   <p>@price</p>
   ```

---