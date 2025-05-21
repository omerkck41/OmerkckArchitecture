# Core.Localization

Modern, modüler ve genişletilebilir, async-first yapı ile geliştirilen .NET 9.0 lokalizasyon kütüphanesi. Feature-based yaklaşım ile çoklu dil desteği, para birimi formatlaması ve tarih/saat yerelleştirmesini kolaylaştırır.

## Özellikler

- 🔄 **Tam Asenkron API** - Tüm metotlar async desteği ile gelir
- 🌟 **Feature-Based Lokalizasyon** - Modüler yapılarda kolay kullanım
- 🌍 **Çoklu dil desteği** ve fallback mekanizmaları
- 💰 **Para birimi formatlaması** kültüre özel kurallarla
- 📅 **Tarih ve saat formatlaması** farklı desenlerle
- 🔄 **Çoklu kaynak sağlayıcıları**: JSON, YAML
- ⚡ **Yüksek performans** önbellekleme desteği ile
- 🔧 **Genişletilebilir mimari** provider pattern ile
- 🔍 **Otomatik kaynak keşfi** - Resources/Locales klasörlerini bulur
- 📁 **FileSystemWatcher desteği** - Kaynakların değişikliklerini izler
- 🚀 **Assembly-based lokalizasyon** - Gömülü kaynakları destekler
- 🌐 **Distributed cache desteği** - Cloud native senaryolar için optimize

## Kurulum

```bash
dotnet add package Core.Localization
```

## Hızlı Başlangıç

### 1. Temel Kurulum

```csharp
using Core.Localization.Extensions;
using System.Globalization;

// Program.cs veya Startup.cs içinde
services.AddCore.Localization(options =>
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

// ASP.NET Core uygulamalarında request localization middleware ekleyin
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("tr-TR"),
        new CultureInfo("en-US"),
        new CultureInfo("fr-FR")
    },
    SupportedUICultures = new List<CultureInfo>
    {
        new CultureInfo("tr-TR"),
        new CultureInfo("en-US"),
        new CultureInfo("fr-FR")
    }
});
```

### 2. Feature-Based Lokalizasyon

```csharp
// Daha basit kurulum için Feature-based yaklaşım kullanabilirsiniz
services.AddFeatureBasedLocalization(options =>
{
    options.DefaultCulture = new CultureInfo("tr-TR");
    options.FallbackCulture = new CultureInfo("en-US");
});
```

### 3. Kullanım

```csharp
// Doğrudan ILocalizationService kullanımı
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
}
```

### 4. String Uzantıları

```csharp
using Core.Localization.Extensions;

// String uzantıları ile yerelleştirme
var greeting = await "Hello".LocalizeAsync(_localizationService);
var userNotFound = await "UserNotFound".LocalizeAsync(_localizationService, "Users");
var welcome = await "Welcome".LocalizeAsync(_localizationService, "Users", "John");

// Dene ve yerelleştir (hata fırlatmaz)
var result = await "MissingKey".TryLocalizeAsync(_localizationService, "Users");
if (result.Success)
{
    // Yerelleştirilmiş değeri kullan
    var value = result.Value;
}
```

## Kaynak Dosyaları

### YAML Dosyaları

Feature klasörleriniz içerisinde YAML dosyaları oluşturun:

```yaml
# Features/Users/Resources/Locales/users.en.yaml
SectionName: Users
Messages:
  UserDoesNotExist: "User doesn't exist"
  UserAlreadyExists: "User already exists"
  UserNameRequired: "Username is required"
  EmailRequired: "Email is required"
  PasswordRequired: "Password is required"
  Welcome: "Welcome, {0}!"

# Features/Users/Resources/Locales/users.tr.yaml
SectionName: Kullanıcılar
Messages:
  UserDoesNotExist: "Kullanıcı mevcut değil"
  UserAlreadyExists: "Kullanıcı zaten mevcut"
  UserNameRequired: "Kullanıcı adı gereklidir"
  EmailRequired: "E-posta gereklidir"
  PasswordRequired: "Şifre gereklidir"
  Welcome: "Hoş geldin, {0}!"
```

### JSON Dosyaları

Alternatif olarak JSON formatını kullanabilirsiniz:

```json
// Features/Users/Resources/Locales/users.en.json
{
    "SectionName": "Users",
    "UserDoesNotExist": "User doesn't exist",
    "UserAlreadyExists": "User already exists",
    "UserNameRequired": "Username is required",
    "EmailRequired": "Email is required",
    "PasswordRequired": "Password is required",
    "Welcome": "Welcome, {0}!"
}

// Features/Users/Resources/Locales/users.tr.json
{
    "SectionName": "Kullanıcılar",
    "UserDoesNotExist": "Kullanıcı mevcut değil",
    "UserAlreadyExists": "Kullanıcı zaten mevcut",
    "UserNameRequired": "Kullanıcı adı gereklidir",
    "EmailRequired": "E-posta gereklidir",
    "PasswordRequired": "Şifre gereklidir",
    "Welcome": "Hoş geldin, {0}!"
}
```

## Formatlamalar

FormatterService ile kapsamlı formatlama işlevleri:

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

## Mimari Entegrasyonları

### Clean Architecture / DDD

Core.Localization, Clean Architecture veya Domain-Driven Design yaklaşımlarına mükemmel uyum sağlar:

```
/src
  /Core
    /Domain
      /Users
        /BusinessRules
          UserBusinessRules.cs  # Lokalizasyon servisini kullanır
    /Application
      /Users
        /Commands
          /CreateUser
            CreateUserCommand.cs
            CreateUserCommandHandler.cs  # Lokalizasyon servisini kullanır
  /Infrastructure
    /Localization  # Core.Localization yapılandırması
```

#### UserBusinessRules.cs

```csharp
public class UserBusinessRules
{
    private readonly ILocalizationService _localizationService;
    
    public UserBusinessRules(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    
    private async Task<BusinessException> CreateBusinessExceptionAsync(string messageKey)
    {
        string message = await _localizationService.GetStringAsync(messageKey, "Users");
        return new BusinessException(message);
    }
    
    public async Task UserShouldExistWhenSelectedAsync(User? user)
    {
        if (user == null)
            throw await CreateBusinessExceptionAsync("UserDoesNotExist");
    }
    
    public async Task EmailMustBeUniqueWhenRegisteredAsync(string email, IUserRepository userRepository)
    {
        if (await userRepository.ExistsAsync(u => u.Email == email))
            throw await CreateBusinessExceptionAsync("UserAlreadyExists");
    }
}
```

### MediatR

MediatR ile entegrasyon örneği:

```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly UserBusinessRules _businessRules;
    private readonly ILocalizationService _localizationService;
    
    public CreateUserCommandHandler(
        IUserRepository userRepository,
        UserBusinessRules businessRules,
        ILocalizationService localizationService)
    {
        _userRepository = userRepository;
        _businessRules = businessRules;
        _localizationService = localizationService;
    }
    
    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validate business rules
        await _businessRules.EmailMustBeUniqueWhenRegisteredAsync(request.Email, _userRepository);
        
        // Create user (implementation details omitted for brevity)
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email
        };
        
        // Return success response with localized message
        return new CreateUserResponse
        {
            UserId = user.Id,
            Message = await _localizationService.GetStringAsync("UserCreatedSuccessfully", "Users", request.UserName)
        };
    }
}
```

### Assembly-Based Lokalizasyon

Assembly içindeki gömülü kaynaklardan lokalizasyon desteği:

```csharp
// Belirli assembly'lerin lokalizasyon kaynaklarını kullan
services.AddAssemblyBasedLocalization(
    typeof(Startup).Assembly,
    typeof(SharedLibrary.Marker).Assembly
);

// Alternatif olarak çalışan uygulamanın assembly'sini kullan
services.AddApplicationLocalization();

// Belirli bir türün yer aldığı assembly'yi kullan
services.AddLocalizationFromAssemblyOf<UserEntity>();

// Gelişmiş ayarlarla assembly seçeneklerini yapılandır
services.AddAdvancedAssemblyLocalization(options => {
    options.AddAssemblyOf<UserEntity>()
           .AddAssembly(someOtherAssembly)
           .AddEntryAssembly();
    
    options.ResourcePaths = new List<string> { "Locales", "Resources" };
    options.EnableDebugLogging = true;
});
```

## Özel Resource Sağlayıcıları

`IResourceProvider` arayüzünü uygulayarak kendi kaynak sağlayıcınızı oluşturabilirsiniz:

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
        var cultureName = culture.Name;
        
        var translation = await _dbContext.Translations
            .FirstOrDefaultAsync(t => 
                t.Key == effectiveKey && 
                t.Culture == cultureName,
                cancellationToken);
            
        return translation?.Value;
    }

    // Diğer metotları uygulayın...
}

// Sağlayıcınızı kaydedin
services.AddResourceProvider<DatabaseResourceProvider>();
```

## Yapılandırma Seçenekleri

```csharp
services.AddCore.Localization(options =>
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
    
    // Resource dosya uzantıları
    options.ResourceFileExtensions = new List<string> { "yaml", "yml", "json" };
    
    // Dosya değişikliklerini izlemek için File System Watcher kullan
    options.UseFileSystemWatcher = true;
    
    // Dağıtık önbellek kullanımını etkinleştir (cloud senaryolar için)
    options.EnableDistributedCache = false;
    
    // Büyük/küçük harf duyarsız anahtar karşılaştırması
    options.UseCaseInsensitiveKeys = true;
});
```

## Performans İpuçları

1. Önbellekleme varsayılan olarak etkin - üretim ortamında kullanın
2. Küçük uygulamalar için `UseFileSystemWatcher = false` değerini kullanın
3. Performans gereksinimleri doğrultusunda `EnableAutoDiscovery` seçeneğini açıp kapatın
4. Cloud ortamları için `EnableDistributedCache = true` değerini kullanın
5. Çok fazla kaynak dosyası için önbellek süresi kısa tutulmalıdır

## En İyi Uygulamalar

1. Kültürden bağımsız anahtarlar kullanın (örn. "Hello" yerine "Hello_en" değil)
2. Kaynak anahtarlarını kültürler arasında tutarlı tutun 
3. Dinamik içerik için yer tutucular kullanın: "Welcome, {0}!"
4. Kaynakları feature veya modüle göre düzenleyin
5. Business kurallarını lokalizasyon servisi ile entegre edin
6. Clean Architecture yaklaşımında, lokalizasyon servisi Application katmanında kullanılmalıdır
7. MediatR kullanıyorsanız, command handler'larınızda lokalizasyon servisini enjekte edin
8. Performans için uygun yapılandırmayı kullanın

## Sürüm Geçmişi

- **1.0.0** - İlk sürüm
  - Tam async API desteği
  - Feature-based lokalizasyon
  - JSON ve YAML kaynak sağlayıcıları
  - Otomatik kaynak keşfi
  - FileSystemWatcher desteği
