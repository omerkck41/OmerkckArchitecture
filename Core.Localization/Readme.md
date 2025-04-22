# Core.Localization

A modern, modular, and extensible localization library for .NET 9.0 that simplifies multi-language support, currency formatting, and date/time localization.

## Features

- 🌍 **Multi-language support** with fallback mechanisms
- 💱 **Currency formatting** with culture-specific rules
- 📅 **Date and time formatting** with different patterns
- 🔄 **Multiple resource providers**: RESX, JSON, YAML
- ⚡ **High performance** with caching support
- 🔧 **Extensible architecture** with provider pattern
- 🧪 **Comprehensive unit tests**


## Quick Start

1. Add Core.Localization to your services:

```csharp
using Core.Localization.Extensions;

// In your Startup.cs or Program.cs
services.AddCoreLocalization(options =>
{
    options.DefaultCulture = new CultureInfo("en-US");
    options.FallbackCulture = new CultureInfo("en-US");
    options.SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("tr-TR"),
        new CultureInfo("fr-FR")
    };
});
```

2. Use the ILocalizationService:

```csharp
public class MyController : Controller
{
    private readonly ILocalizationService _localization;
    private readonly IFormatterService _formatter;

    public MyController(ILocalizationService localization, IFormatterService formatter)
    {
        _localization = localization;
        _formatter = formatter;
    }

    public IActionResult Index()
    {
        // Get localized string
        var greeting = _localization.GetString("Hello", new CultureInfo("tr-TR"));
        
        // Format a date
        var formattedDate = _formatter.FormatDate(DateTime.Now, culture: new CultureInfo("fr-FR"));
        
        // Format currency
        var formattedPrice = _formatter.FormatCurrency(99.99m, "EUR", new CultureInfo("fr-FR"));
        
        // Use formatted strings
        var welcome = _localization.GetString("Welcome", "John");
        
        return View((greeting, formattedDate, formattedPrice, welcome));
    }
}
```

3. Using Extension Methods:

```csharp
using Core.Localization.Extensions;

// Localize strings directly
var greeting = "Hello".Localize(_localizationService);
var welcome = "Welcome".Localize(_localizationService, "John");

// Try to localize (won't throw if key is missing)
if ("MissingKey".TryLocalize(_localizationService, out var value))
{
    // Use the localized value
}
```

## Configuration Options

The library can be configured through the `LocalizationOptions` class:

```csharp
services.AddCoreLocalization(options =>
{
    // Default culture when none is specified
    options.DefaultCulture = new CultureInfo("en-US");
    
    // Fallback culture when resource not found
    options.FallbackCulture = new CultureInfo("en-US");
    
    // Supported cultures by the application
    options.SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("tr-TR"),
        new CultureInfo("fr-FR")
    };
    
    // Whether to use fallback culture when resource not found
    options.UseFallbackCulture = true;
    
    // Whether to throw exception when resource not found
    options.ThrowOnMissingResource = false;
    
    // Enable/disable caching
    options.EnableCaching = true;
    
    // Cache expiration time
    options.CacheExpiration = TimeSpan.FromHours(1);
    
    // Resource file locations
    options.ResourcePaths = new List<string> { "Resources" };
    
    // Enable file watching for auto-reload
    options.EnableResourceFileWatching = true;
});
```

## Resource Providers

The library supports multiple resource providers out of the box:

### JSON Files

Create JSON files in your Resources directory:

```json
// Resources/resources.en-US.json
{
    "Hello": "Hello",
    "Welcome": "Welcome, {0}!",
    "Goodbye": "Goodbye"
}

// Resources/resources.tr-TR.json
{
    "Hello": "Merhaba",
    "Welcome": "Hoş geldin, {0}!",
    "Goodbye": "Güle güle"
}
```

### YAML Files

Create YAML files in your Resources directory:

```yaml
# Resources/resources.en-US.yaml
Hello: Hello
Welcome: Welcome, {0}!
Goodbye: Goodbye

# Resources/resources.tr-TR.yaml
Hello: Merhaba
Welcome: Hoş geldin, {0}!
Goodbye: Güle güle
```

### RESX Files

Traditional .NET resource files are also supported. Place your .resx files in the project and they will be automatically loaded.

### Custom Resource Providers

You can create your own resource providers by implementing the `IResourceProvider` interface:

```csharp
public class MyCustomProvider : ResourceProviderBase
{
    public override string? GetString(string key, CultureInfo culture)
    {
        // Your custom logic here
    }

    public override IEnumerable<string> GetAllKeys(CultureInfo culture)
    {
        // Your custom logic here
    }
}

// Register your provider
services.AddResourceProvider<MyCustomProvider>();
```

## Formatting Services

The library includes comprehensive formatting services:

```csharp
var formatter = serviceProvider.GetRequiredService<IFormatterService>();

// Format dates
var date = formatter.FormatDate(DateTime.Now, "yyyy-MM-dd", new CultureInfo("en-US"));

// Format numbers
var number = formatter.FormatNumber(1234.56m, "N2", new CultureInfo("tr-TR"));

// Format currency
var currency = formatter.FormatCurrency(99.99m, "USD", new CultureInfo("en-US"));

// Format percentage
var percentage = formatter.FormatPercentage(0.1234m, 2, new CultureInfo("en-US"));

// Parse culture-specific strings
var parsedDate = formatter.ParseDate("25/12/2025", new CultureInfo("fr-FR"));
var parsedNumber = formatter.ParseNumber("1.234,56", new CultureInfo("de-DE"));
var parsedCurrency = formatter.ParseCurrency("$1,234.56", new CultureInfo("en-US"));
```

## Advanced Usage

### Culture Information Extensions

```csharp
var culture = new CultureInfo("en-US");

// Get language code
var langCode = culture.GetLanguageCode(); // "en"

// Check if culture is RTL
var isRtl = culture.IsRightToLeft(); // false

// Get parent cultures
var parents = culture.GetParentCultures(); // [en]

// Check if cultures are related
var isRelated = culture.IsRelatedTo(new CultureInfo("en-GB")); // true
```

### Dynamic Resource Loading

The library supports dynamic resource loading and file watching:

```csharp
// Resources are automatically reloaded when files change
// if EnableResourceFileWatching is true

// Or manually reload resources
var provider = serviceProvider.GetService<IResourceProvider>();
if (provider.SupportsDynamicReload)
{
    await provider.ReloadAsync();
}
```

## Performance Considerations

- Caching is enabled by default for better performance
- Resource providers are prioritized to check faster sources first
- File watching can be disabled in production for better performance
- Use compiled RESX files for best performance in production

## Best Practices

1. Use culture-neutral keys (e.g., "Hello" instead of "Hello_en")
2. Keep resource keys consistent across cultures
3. Use placeholders for dynamic content: "Welcome, {0}!"
4. Organize resources by feature or module
5. Use appropriate resource providers for your deployment scenario
6. Enable caching in production environments
7. Use fallback cultures to ensure all content is available

## Türkçe Kullanım Kılavuzu

### Genel Bakış

Core.Localization, .NET 9.0 uygulamaları için geliştirilmiş modern, modüler ve genişletilebilir bir lokalizasyon kütüphanesidir. Çoklu dil desteği, para birimi formatlaması ve tarih/saat yerelleştirmesi için kapsamlı çözümler sunar.

### Kurulum

```bash
dotnet add package Core.Localization
```

### Temel Kullanım

1. Servis yapılandırması:

```csharp
// Program.cs veya Startup.cs içinde
services.AddCoreLocalization(options =>
{
    options.DefaultCulture = new CultureInfo("tr-TR");
    options.FallbackCulture = new CultureInfo("en-US");
    options.SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("tr-TR"),
        new CultureInfo("en-US"),
        new CultureInfo("fr-FR")
    };
});
```

2. Controller veya servis içinde kullanım:

```csharp
public class HomeController : Controller
{
    private readonly ILocalizationService _localization;
    private readonly IFormatterService _formatter;

    public HomeController(ILocalizationService localization, IFormatterService formatter)
    {
        _localization = localization;
        _formatter = formatter;
    }

    public IActionResult Index()
    {
        // Mevcut kültüre göre çeviri al
        var karsilama = _localization.GetString("Hello");
        
        // Belirli kültür için çeviri al
        var fransizca = _localization.GetString("Hello", new CultureInfo("fr-FR"));
        
        // Parametreler ile çeviri al
        var hosgeldin = _localization.GetString("Welcome", "Kullanıcı");
        
        // Para birimi formatla
        var fiyat = _formatter.FormatCurrency(1234.56m);
        
        // Tarih formatla
        var tarih = _formatter.FormatDate(DateTime.Now);
        
        return View(new { Karsilama = karsilama, Hosgeldin = hosgeldin, Fiyat = fiyat, Tarih = tarih });
    }
}
```

### Kaynak Dosyaları

`Resources` klasörü altında aşağıdaki dosyaları oluşturarak çevirileri tanımlayabilirsiniz:

- JSON formatı: `resources.tr-TR.json`, `resources.en-US.json`
- YAML formatı: `resources.tr-TR.yaml`, `resources.en-US.yaml`
- RESX formatı: Standart .NET kaynak dosyaları

### Özelleştirme

Özel kaynak sağlayıcısı oluşturarak veritabanı, uzak API veya farklı formatlardaki dosyalardan çevirileri alabilirsiniz:

```csharp
public class DatabaseResourceProvider : ResourceProviderBase
{
    private readonly AppDbContext _dbContext;

    public DatabaseResourceProvider(AppDbContext dbContext) : base(priority: 500)
    {
        _dbContext = dbContext;
    }

    public override string? GetString(string key, CultureInfo culture)
    {
        return _dbContext.Translations
            .FirstOrDefault(t => t.Key == key && t.Culture == culture.Name)
            ?.Value;
    }

    public override IEnumerable<string> GetAllKeys(CultureInfo culture)
    {
        return _dbContext.Translations
            .Where(t => t.Culture == culture.Name)
            .Select(t => t.Key);
    }
}

// Servis kaydı
services.AddResourceProvider<DatabaseResourceProvider>();
```

Daha fazla bilgi için örnek projeyi inceleyebilirsiniz.

### Modüler Kullanım Yaklaşımı

Core.Localization kütüphanesi, temiz mimari veya DDD ile geliştirilen uygulamalarda her özellik/modül için ayrı kaynak dosyaları kullanan modüler bir yapıyı destekler. Özellikle feature-based bir yapıya sahip projelerde bu yaklaşım daha düzenli ve yönetilebilir bir şekilde lokalizasyonu yapmanızı sağlar.

Birkaç modüler kullanım yaklaşımı:

#### 1. Her özellik için ayrı resource paths tanımlama:

```csharp
services.AddCoreLocalization(options =>
{
    options.ResourcePaths = new List<string> 
    { 
        "Application/Features/Users/Resources",
        "Application/Features/Orders/Resources",
        "Application/Features/Products/Resources"
    };
});
```

#### 2. Özelliğe göre resource dosyalarını adlandırma:

```
users.tr-TR.json
users.en-US.json
orders.tr-TR.json
orders.en-US.json
```

#### 3. Resource key'lerde ön ekler kullanma:

```json
// users.tr-TR.json
{
  "users.title": "Kullanıcı Yönetimi",
  "users.create": "Kullanıcı Oluştur",
  "users.edit": "Kullanıcı Düzenle"
}

// orders.tr-TR.json
{
  "orders.title": "Sipariş Yönetimi",
  "orders.create": "Sipariş Oluştur",
  "orders.status": "Sipariş Durumu"
}
```

Kullanım:
```csharp
var usersTitle = _localization.GetString("users.title");
var ordersTitle = _localization.GetString("orders.title");
```

#### 4. YAML ile hiyerarşik yapı kullanma:

```yaml
# resources.tr-TR.yaml
Users:
  Title: "Kullanıcı Yönetimi"
  Create: "Kullanıcı Oluştur"
  Edit: "Kullanıcı Düzenle"

Orders:
  Title: "Sipariş Yönetimi"
  Create: "Sipariş Oluştur"
  Status: "Sipariş Durumu"
```

Kullanım:
```csharp
var usersTitle = _localization.GetString("Users.Title");
var ordersTitle = _localization.GetString("Orders.Title");
```

#### 5. Her özellik için wrapper servisler oluşturma:

```csharp
public class UsersLocalizationService
{
    private readonly ILocalizationService _localization;
    
    public UsersLocalizationService(ILocalizationService localization)
    {
        _localization = localization;
    }
    
    public string GetTitle() => _localization.GetString("users.title");
    public string GetCreateLabel() => _localization.GetString("users.create");
    public string GetEditLabel() => _localization.GetString("users.edit");
}
```
