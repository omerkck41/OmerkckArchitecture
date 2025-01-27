# Core.ToolKit.Localization

**Core.ToolKit.Localization**, .NET projelerinde lokalizasyon (yerelleştirme), tarih-saat formatlama ve para birimi formatlama işlemlerini kolaylaştırmak için geliştirilmiş bir kütüphanedir. Bu kütüphane, farklı kültürler için çeviri yönetimi, tarih-saat ve para birimi formatlama işlemlerini merkezi bir şekilde yönetmeyi sağlar.

## Nedir?

**Core.ToolKit.Localization**, aşağıdaki temel işlevleri sağlayan bir araç setidir:

1. **Lokalizasyon (Yerelleştirme)**: Farklı diller için çeviri yönetimi.
2. **Tarih ve Saat Formatlama**: Kültüre özgü tarih ve saat formatlama.
3. **Para Birimi Formatlama**: Kültüre özgü para birimi formatlama ve çevrim işlemleri.

Bu kütüphane, büyük ölçekli projelerde farklı diller ve kültürler için uyumlu çözümler sunar.

## Neden Kullanılır?

- **Çoklu Dil Desteği**: Farklı diller için çeviri yönetimi sağlar.
- **Kültüre Özgü Formatlama**: Tarih, saat ve para birimi formatlamalarını kültüre özgü olarak yönetir.
- **Esneklik**: Özel formatlar ve semboller ile esnek bir kullanım sunar.
- **Performans**: Çeviriler ve formatlama işlemleri için cache mekanizması ile performansı artırır.

## Avantajları

- **Merkezi Yönetim**: Tüm lokalizasyon ve formatlama işlemleri tek bir yerden yönetilir.
- **Genişletilebilirlik**: Yeni kültürler ve formatlar kolayca eklenebilir.
- **Test Edilebilirlik**: Bağımlılıklar enjekte edilerek test edilebilir bir yapı sunar.
- **Async Destek**: Asenkron işlemler ile performans artışı sağlar.

## Projeye Ekleme ve Ayarlar

### 1. Projeye Ekleme

**Core.ToolKit.Localization** kütüphanesini projenize eklemek için aşağıdaki adımları izleyin:

1. **Program.cs** dosyasında kütüphaneyi yapılandırın:

   ```csharp
   using Core.ToolKit.Localization;

   var builder = WebApplication.CreateBuilder(args);

   // Lokalizasyon ayarlarını yapılandır
   builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

   var app = builder.Build();

   // Varsayılan kültürü ayarla
   LocalizationHelper.DefaultCulture = "en-US";

   // Çeviri dosyalarını yükle
   var translationFiles = new Dictionary<string, string>
   {
       { "en-US", "Resources/translations.en-US.json" },
       { "tr-TR", "Resources/translations.tr-TR.json" }
   };

   await LocalizationHelper.LoadTranslationsAsync(translationFiles);

   app.Run();
   ```

3. **appsettings.json** dosyasında kültür ve çeviri dosyalarını yapılandırın:

   ```json
   {
     "Localization": {
       "DefaultCulture": "en-US",
       "TranslationFiles": {
         "en-US": "Resources/translations.en-US.json",
         "tr-TR": "Resources/translations.tr-TR.json"
       }
     }
   }
   ```

### 2. Çeviri Dosyalarını Hazırlama

Çeviri dosyalarını JSON formatında hazırlayın ve `Resources` klasörüne ekleyin. Örneğin:

**translations.en-US.json**:

```json
{
  "WelcomeMessage": "Welcome to our application!",
  "GoodbyeMessage": "Goodbye, {0}!"
}
```

**translations.tr-TR.json**:

```json
{
  "WelcomeMessage": "Uygulamamıza hoş geldiniz!",
  "GoodbyeMessage": "Güle güle, {0}!"
}
```

## Detaylı Kullanım Örnekleri

### 1. Lokalizasyon (Çeviri) Kullanımı

```csharp
using Core.ToolKit.Localization;

// Varsayılan kültürü ayarla
LocalizationHelper.DefaultCulture = "en-US";

// Çeviri yap
var welcomeMessage = LocalizationHelper.Translate("WelcomeMessage");
Console.WriteLine(welcomeMessage); // "Welcome to our application!"

// Parametreli çeviri
var goodbyeMessage = LocalizationHelper.Translate("GoodbyeMessage", "John");
Console.WriteLine(goodbyeMessage); // "Goodbye, John!"

// Kültürü değiştir
LocalizationHelper.DefaultCulture = "tr-TR";

// Yeni kültürde çeviri yap
welcomeMessage = LocalizationHelper.Translate("WelcomeMessage");
Console.WriteLine(welcomeMessage); // "Uygulamamıza hoş geldiniz!"

goodbyeMessage = LocalizationHelper.Translate("GoodbyeMessage", "Ahmet");
Console.WriteLine(goodbyeMessage); // "Güle güle, Ahmet!"
```

### 2. Tarih ve Saat Formatlama

```csharp
using Core.ToolKit.Localization;
using System.Globalization;

// Varsayılan kültürü ayarla
LocalizationHelper.DefaultCulture = "en-US";

var dateTime = DateTime.Now;

// Tarih formatlama
var formattedDate = DateTimeFormatter.FormatDate(dateTime);
Console.WriteLine(formattedDate); // "10/15/2023"

// Saat formatlama
var formattedTime = DateTimeFormatter.FormatTime(dateTime);
Console.WriteLine(formattedTime); // "3:45 PM"

// Özel formatlama
var customFormattedDate = DateTimeFormatter.FormatDate(dateTime, "yyyy-MM-dd");
Console.WriteLine(customFormattedDate); // "2023-10-15"

// Kültürü değiştir
LocalizationHelper.DefaultCulture = "tr-TR";

// Yeni kültürde formatlama
formattedDate = DateTimeFormatter.FormatDate(dateTime);
Console.WriteLine(formattedDate); // "15.10.2023"

formattedTime = DateTimeFormatter.FormatTime(dateTime);
Console.WriteLine(formattedTime); // "15:45"
```

### 3. Para Birimi Formatlama

```csharp
using Core.ToolKit.Localization;
using System.Globalization;

// Varsayılan kültürü ayarla
LocalizationHelper.DefaultCulture = "en-US";

var amount = 1234.56m;

// Para birimi formatlama
var formattedCurrency = CurrencyFormatter.Format(amount);
Console.WriteLine(formattedCurrency); // "$1,234.56"

// Özel sembol ile formatlama
var customFormattedCurrency = CurrencyFormatter.Format(amount, "€");
Console.WriteLine(customFormattedCurrency); // "€1,234.56"

// Kültürü değiştir
LocalizationHelper.DefaultCulture = "tr-TR";

// Yeni kültürde formatlama
formattedCurrency = CurrencyFormatter.Format(amount);
Console.WriteLine(formattedCurrency); // "1.234,56 TL"

// Para birimi çevrimi
var currencyString = "1.234,56 TL";
var parsedAmount = CurrencyFormatter.Parse(currencyString);
Console.WriteLine(parsedAmount); // 1234.56
```

## Sonuç

**Core.ToolKit.Localization**, .NET projelerinde lokalizasyon, tarih-saat ve para birimi formatlama işlemlerini kolaylaştıran güçlü bir kütüphanedir. Bu kütüphane ile farklı kültürler ve diller için uyumlu çözümler sunabilir, projelerinizi daha esnek ve genişletilebilir hale getirebilirsiniz.