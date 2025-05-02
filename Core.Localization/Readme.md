# Core.Localization

Modern, modüler ve genişletilebilir, async-first yapı ile geliştirilen .NET 9.0 lokalizasyon kütüphanesi. Feature-based yaklaşım ile çoklu dil desteği, para birimi formatlaması ve tarih/saat yerelleştirmesini kolaylaştırır.

## Özellikler

- 🔄 **Tam Asenkron API** - tüm metotlar async desteği ile gelir
- 🌟 **Feature-Based Lokalizasyon** - modüler yapılarda kolay kullanım
- 🌍 **Çoklu dil desteği** ve fallback mekanizmaları
- 💱 **Para birimi formatlaması** kültüre özel kurallarla
- 📅 **Tarih ve saat formatlaması** farklı desenlerle
- 🔄 **Çoklu kaynak sağlayıcıları**: JSON, YAML
- ⚡ **Yüksek performans** önbellekleme desteği ile
- 🔧 **Genişletilebilir mimari** provider pattern ile
- 🧪 **Kapsamlı birim testleri**
- 🔍 **Otomatik kaynak keşfi** - Resources/Locales klasörlerini bulur
- 📁 **FileSystemWatcher desteği** - kaynakların değişikliklerini izler


## Hızlı Başlangıç

1. Core.Localization'ı servislerinize ekleyin:

```csharp
using Core.Localization.Extensions;

// Program.cs'de
services.AddFeatureBasedLocalization(options =>
{
    options.DefaultCulture = new CultureInfo("tr-TR");
    options.FallbackCulture = new CultureInfo("en-US");
    options.SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("tr-TR"),
        new CultureInfo("en-US"),
        new CultureInfo("fr-FR")
    };
    options.ResourcePaths = new List<string> { "Features" };
});
```

2. ILocalizationService kullanımı:

```csharp
public class UsersController : Controller
{
    private readonly ILocalizationService _localizationService;

    public UsersController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public async Task<IActionResult> Index()
    {
        // Basit yerelleştirme
        var greeting = await _localizationService.GetStringAsync("Hello");
        
        // Feature-based yerelleştirme
        var userNotFound = await _localizationService.GetStringAsync("UserNotFound", "Users");
        
        // Formatlı mesajlar
        var welcome = await _localizationService.GetStringAsync("Welcome", "Users", "John");
        
        return View(new { Greeting = greeting, UserNotFound = userNotFound, Welcome = welcome });
    }
    
    private async Task ThrowBusinessException(string messageKey)
    {
        // Feature-based lokalizasyon örneği
        string message = await _localizationService.GetStringAsync(messageKey, "Users");
        throw new BusinessException(message);
    }
    
    public async Task UserShouldBeExistsWhenSelected(User? user)
    {
        if (user == null)
            await ThrowBusinessException("UserDontExists");
    }
}
```

3. Uzantı metotları kullanımı:

```csharp
using Core.Localization.Extensions;

// String uzantıları ile yerelleştirme
var greeting = await "Hello".LocalizeAsync(_localizationService);
var userNotFound = await "UserNotFound".LocalizeAsync(_localizationService, "Users");
var welcome = await "Welcome".LocalizeAsync(_localizationService, "Users", "John");

// Dene ve yerelleştir (hata fırlatmaz)
var result = await "MissingKey".TryLocalizeAsync(_localizationService, "Users");
if (result.success)
{
    // Yerelleştirilmiş değeri kullan
    var value = result.value;
}
```

## Yapılandırma Seçenekleri

Kütüphane `LocalizationOptions` sınıfı üzerinden yapılandırılabilir:

```csharp
services.AddFeatureBasedLocalization(options =>
{
    // Belirtilmediğinde kullanılacak varsayılan kültür
    options.DefaultCulture = new CultureInfo("tr-TR");
    
    // Kaynak bulunamadığında kullanılacak yedek kültür
    options.FallbackCulture = new CultureInfo("en-US");
    
    // Uygulama tarafından desteklenen kültürler
    options.SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("tr-TR"),
        new CultureInfo("en-US"),
        new CultureInfo("fr-FR")
    };
    
    // Kaynak bulunamadığında yedek kültürü kullanıp kullanmama
    options.UseFallbackCulture = true;
    
    // Kaynak bulunamadığında istisna fırlatıp fırlatmama
    options.ThrowOnMissingResource = false;
    
    // Önbelleklemeyi etkinleştir/devre dışı bırak
    options.EnableCaching = true;
    
    // Önbellek süre aşımı
    options.CacheExpiration = TimeSpan.FromHours(1);
    
    // Kaynak dosyası konumları - bu, Feature klasörlerinin bulunduğu kök dizinler olabilir
    options.ResourcePaths = new List<string> { "Features" };
    
    // Feature yolları için dizin deseni
    options.FeatureDirectoryPattern = "**/Resources/Locales";
    
    // Feature dosyaları için dosya deseni
    options.FeatureFilePattern = "{section}.{culture}.{extension}";
    
    // Belirtilmediğinde kullanılacak varsayılan section
    options.DefaultSection = "Messages";
    
    // Feature kaynakları için otomatik keşfi etkinleştir
    options.EnableAutoDiscovery = true;
    
    // Auto-reload aralığı
    options.AutoReloadInterval = TimeSpan.FromMinutes(5);
    
    // Dosya değişikliklerini izlemek için File System Watcher kullan
    options.UseFileSystemWatcher = true;
});
```

## Resource Sağlayıcıları

Kütüphane kutudan çıktığı haliyle birden çok kaynak sağlayıcısını destekler:

### JSON Dosyaları

Feature klasörleriniz içerisinde JSON dosyaları oluşturun:

```json
// Features/Users/Resources/Locales/users.en.json
{
    "SectionName": "Users",
    "UserDontExists": "User doesn't exist",
    "UserAlreadyExists": "User already exists",
    "UserNameRequired": "Username is required",
    "EmailRequired": "Email is required",
    "PasswordRequired": "Password is required",
    "Welcome": "Welcome, {0}!"
}

// Features/Users/Resources/Locales/users.tr.json
{
    "SectionName": "Kullanıcılar",
    "UserDontExists": "Kullanıcı mevcut değil",
    "UserAlreadyExists": "Kullanıcı zaten mevcut",
    "UserNameRequired": "Kullanıcı adı gereklidir",
    "EmailRequired": "E-posta gereklidir",
    "PasswordRequired": "Şifre gereklidir",
    "Welcome": "Hoş geldin, {0}!"
}
```

### YAML Dosyaları

Feature klasörleriniz içerisinde YAML dosyaları oluşturun:

```yaml
# Features/Users/Resources/Locales/users.en.yaml
SectionName: Users
Messages:
  UserDontExists: "User doesn't exist"
  UserAlreadyExists: "User already exists"
  UserNameRequired: "Username is required"
  EmailRequired: "Email is required"
  PasswordRequired: "Password is required"
  Welcome: "Welcome, {0}!"

# Features/Users/Resources/Locales/users.tr.yaml
SectionName: Kullanıcılar
Messages:
  UserDontExists: "Kullanıcı mevcut değil"
  UserAlreadyExists: "Kullanıcı zaten mevcut"
  UserNameRequired: "Kullanıcı adı gereklidir"
  EmailRequired: "E-posta gereklidir"
  PasswordRequired: "Şifre gereklidir"
  Welcome: "Hoş geldin, {0}!"
```

### Özel Resource Sağlayıcıları

`IResourceProvider` arayüzünü uygulayarak kendi resource sağlayıcınızı oluşturabilirsiniz:

```csharp
public class DatabaseResourceProvider : ResourceProviderBase
{
    private readonly AppDbContext _dbContext;

    public DatabaseResourceProvider(AppDbContext dbContext) : base(priority: 500)
    {
        _dbContext = dbContext;
    }

    public override async Task<string?> GetStringAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        var effectiveKey = GetEffectiveKey(key, section);
        var cultureName = GetNormalizedCultureCode(culture);
        
        var translation = await _dbContext.Translations
            .FirstOrDefaultAsync(t => 
                t.Key == effectiveKey && 
                t.Culture == cultureName,
                cancellationToken);
            
        return translation?.Value;
    }

    public override async Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        var cultureName = GetNormalizedCultureCode(culture);
        var prefix = section != null ? $"{section}." : "";
        
        var keys = await _dbContext.Translations
            .Where(t => t.Culture == cultureName && 
                   (string.IsNullOrEmpty(section) || t.Key.StartsWith(prefix)))
            .Select(t => string.IsNullOrEmpty(section) ? t.Key : t.Key.Substring(prefix.Length))
            .ToListAsync(cancellationToken);
            
        return keys;
    }
    
    public override async Task<IEnumerable<string>> GetAllSectionsAsync(CultureInfo culture, CancellationToken cancellationToken = default)
    {
        var cultureName = GetNormalizedCultureCode(culture);
        
        var sections = await _dbContext.Translations
            .Where(t => t.Culture == cultureName && t.Key.Contains("."))
            .Select(t => t.Key.Split('.')[0])
            .Distinct()
            .ToListAsync(cancellationToken);
            
        return sections;
    }
}

// Sağlayıcınızı kaydedin
services.AddResourceProvider<DatabaseResourceProvider>();
```

## Formatlamalar

Kütüphane, FormatterService ile kapsamlı formatlama işlevleri sunar:

```csharp
var formatter = serviceProvider.GetRequiredService<IFormatterService>();

// Tarihleri formatla
var date = await formatter.FormatDateAsync(DateTime.Now, "yyyy-MM-dd", new CultureInfo("en-US"));

// Sayıları formatla
var number = await formatter.FormatNumberAsync(1234.56m, "N2", new CultureInfo("tr-TR"));

// Para birimlerini formatla
var currency = await formatter.FormatCurrencyAsync(99.99m, "USD", new CultureInfo("en-US"));

// Yüzdeleri formatla
var percentage = await formatter.FormatPercentageAsync(0.1234m, 2, new CultureInfo("en-US"));

// Kültüre özgü dizgileri ayrıştır
var parsedDate = await formatter.ParseDateAsync("25/12/2025", new CultureInfo("fr-FR"));
var parsedNumber = await formatter.ParseNumberAsync("1.234,56", new CultureInfo("de-DE"));
var parsedCurrency = await formatter.ParseCurrencyAsync("$1,234.56", new CultureInfo("en-US"));
```

## Feature-Based Lokalizasyon

Core.Localization kütüphanesi, clean architecture veya DDD ile geliştirilen uygulamalarda her feature/modül için ayrı kaynak dosyaları kullanan modüler bir yapıyı destekler. Bu yaklaşım özellikle feature-based bir yapıya sahip projelerde lokalizasyonu daha düzenli ve yönetilebilir kılar.

### Klasör Yapısı

Önerilen klasör yapısı:

```
/Features
  /Users
    /Resources
      /Locales
        users.en.yaml
        users.tr.yaml
  /Orders
    /Resources
      /Locales
        orders.en.yaml
        orders.tr.yaml
  /Products
    /Resources
      /Locales
        products.en.yaml
        products.tr.yaml
```

### Kullanım

```csharp
// Business Logic
public class UserBusinessRules
{
    private readonly ILocalizationService _localizationService;
    
    public UserBusinessRules(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    
    private async Task ThrowBusinessException(string messageKey)
    {
        string message = await _localizationService.GetStringAsync(messageKey, "Users");
        throw new BusinessException(message);
    }
    
    public async Task UserShouldBeExistsWhenSelected(User? user)
    {
        if (user == null)
            await ThrowBusinessException("UserDontExists");
    }
    
    public async Task UserEmailMustBeUniqueWhenInserted(string email)
    {
        if (await _userRepository.ExistsAsync(u => u.Email == email))
            await ThrowBusinessException("UserAlreadyExists");
    }
}
```

### Otomatik Kaynak Keşfi

Kütüphane, tüm feature klasörlerini otomatik olarak tarayıp kaynak dosyalarını bulabilir:

```csharp
services.AddFeatureBasedLocalization(options =>
{
    options.ResourcePaths = new List<string> { "Features" };
    options.EnableAutoDiscovery = true;
});
```

## Performans İyileştirmeleri

- Önbellekleme varsayılan olarak etkindir
- Kaynak sağlayıcıları daha hızlı kaynakları ilk önce kontrol etmek için önceliklendirilir
- Dosya izleme üretim ortamında daha iyi performans için devre dışı bırakılabilir
- `EnableAutoDiscovery` seçeneği, performans gereksinimleri doğrultusunda açılıp kapatılabilir

## En İyi Uygulamalar

1. Kültürden bağımsız anahtarlar kullanın (örn. "Hello" yerine "Hello_en" değil)
2. Kaynak anahtarlarını kültürler arasında tutarlı tutun
3. Dinamik içerik için yer tutucular kullanın: "Welcome, {0}!"
4. Kaynakları feature veya modüle göre düzenleyin
5. Dağıtım senaryonuz için uygun kaynak sağlayıcıları kullanın
6. Üretim ortamlarında önbelleklemeyi etkinleştirin
7. Tüm içeriğin kullanılabilir olmasını sağlamak için fallback kültürleri kullanın

## Örnek Bir Feature-Based Yaklaşım

Users özelliği için bir örnek:

```
/Features/Users
  /Commands
    /CreateUser
      CreateUserCommand.cs
      CreateUserCommandHandler.cs
      CreateUserCommandValidator.cs
  /Queries
    /GetUserById
      GetUserByIdQuery.cs
      GetUserByIdQueryHandler.cs
  /Resources
    /Locales
      users.en.yaml
      users.tr.yaml
  UserBusinessRules.cs
  UsersMessages.cs  // Sabit anahtar tanımlamaları
```

`UsersMessages.cs` sınıfı:

```csharp
public static class UsersMessages
{
    public const string SectionName = "Users";
    
    public const string UserDontExists = "UserDontExists";
    public const string UserAlreadyExists = "UserAlreadyExists";
    public const string UserNameRequired = "UserNameRequired";
    public const string EmailRequired = "EmailRequired";
    public const string PasswordRequired = "PasswordRequired";
    // ...
}
```

`UserBusinessRules.cs` içindeki kullanım:

```csharp
public class UserBusinessRules
{
    private readonly ILocalizationService _localizationService;
    
    public UserBusinessRules(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    
    private async Task ThrowBusinessException(string messageKey)
    {
        string message = await _localizationService.GetStringAsync(messageKey, UsersMessages.SectionName);
        throw new BusinessException(message);
    }
    
    public async Task UserShouldBeExistsWhenSelected(User? user)
    {
        if (user == null)
            await ThrowBusinessException(UsersMessages.UserDontExists);
    }
}
```

## Sürüm Geçmişi

- **1.0.0** - İlk sürüm
  - Tam async API desteği
  - Feature-based lokalizasyon
  - JSON ve YAML kaynak sağlayıcıları
  - Otomatik kaynak keşfi
  - FileSystemWatcher desteği

## Lisans

MIT
