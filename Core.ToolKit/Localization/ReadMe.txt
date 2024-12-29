# Core.Toolkit Localization Module

**Localization Module**, metin çevirileri, tarih/saat biçimlendirme ve para birimi işleme işlemleri için kapsamlı araçlar sunar. Varsayılan kültür desteklenir ve esnek bir şekilde özelleştirilebilir.

---

## **1. LocalizationHelper**

### **Açıklama**
JSON dosyalarından çevirileri yükler ve kültürlere duyarlı metin çevirilerini sağlar.

### **Kullanım**

#### **Çeviri Dosyalarını Yükleme**
```csharp
using Core.Toolkit.Localization;

var translations = new Dictionary<string, string>
{
    { "en-US", "path/to/en-us.json" },
    { "tr-TR", "path/to/tr-tr.json" }
};

await LocalizationHelper.LoadTranslationsAsync(translations);
```

#### **Varsayılan Kültürü Ayarlama**
```csharp
LocalizationHelper.DefaultCulture = "en-US";
```

#### **Çeviri Yapma**
```csharp
string translatedText = LocalizationHelper.Translate("Hello", "John");
Console.WriteLine(translatedText); // Örneğin: "Hello, John"
```

#### **Çevirileri Temizleme**
```csharp
LocalizationHelper.ClearTranslations();
```

---

## **2. DateTimeFormatter**

### **Açıklama**
Tarih ve saatleri varsayılan kültüre göre veya özel formatlarla biçimlendirir.

### **Kullanım**

#### **Tarih Formatlama**
```csharp
using Core.Toolkit.Localization;

DateTime now = DateTime.Now;
string formattedDate = DateTimeFormatter.FormatDate(now);
Console.WriteLine(formattedDate); // Örneğin: "12/31/2024"
```

#### **Saat Formatlama**
```csharp
string formattedTime = DateTimeFormatter.FormatTime(now);
Console.WriteLine(formattedTime); // Örneğin: "23:59"
```

#### **Tarih ve Saat Formatlama**
```csharp
string formattedDateTime = DateTimeFormatter.FormatDateTime(now, "MMMM dd, yyyy HH:mm:ss");
Console.WriteLine(formattedDateTime); // Örneğin: "December 31, 2024 23:59:59"
```

---

## **3. CurrencyFormatter**

### **Açıklama**
Para birimlerini varsayılan kültüre göre biçimlendirir ve analiz eder.

### **Kullanım**

#### **Para Birimi Formatlama**
```csharp
using Core.Toolkit.Localization;

decimal amount = 1234.56m;
string formattedCurrency = CurrencyFormatter.Format(amount);
Console.WriteLine(formattedCurrency); // Örneğin: "$1,234.56"
```

#### **Para Birimi Formatlama (Özel Sembol)**
```csharp
string customCurrency = CurrencyFormatter.Format(amount, "€");
Console.WriteLine(customCurrency); // Örneğin: "€1,234.56"
```

#### **Para Birimi Çözme**
```csharp
string currencyString = "$1,234.56";
decimal parsedValue = CurrencyFormatter.Parse(currencyString);
Console.WriteLine(parsedValue); // Örneğin: 1234.56
```

---

## **Özellikler ve Avantajlar**

- **Esnek Çeviriler**: JSON tabanlı birden fazla dil desteği.
- **Zengin Biçimlendirme**: Tarih, saat ve para biriminde özelleştirilmiş formatlar.
- **Varsayılan Kültür Desteği**: Merkezi bir noktadan kültür ayarlarını yönetme.
- **Performans**: Optimize edilmiş ve büyük projelere uygun.

---
