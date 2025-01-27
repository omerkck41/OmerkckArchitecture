# Core.ToolKit.Security Kütüphanesi

## **Nedir?**
`Core.ToolKit.Security`, .NET projelerinde güvenlikle ilgili yaygın işlemleri kolaylaştırmak için geliştirilmiş bir kütüphanedir. Bu kütüphane, metinlerin maskelenmesi, Base64 encoding/decoding, input sanitization, ve çeşitli validasyon işlemlerini içerir. Büyük ölçekli projelerde güvenlik ihtiyaçlarını karşılamak için esnek ve genişletilebilir bir yapı sunar.

---

## **Neden Kullanılır?**
- **Güvenlik İhtiyaçları:** Kullanıcı girdilerinin sanitize edilmesi, hassas bilgilerin maskelenmesi ve verilerin güvenli bir şekilde encode/decode edilmesi gibi temel güvenlik ihtiyaçlarını karşılar.
- **Kod Tekrarını Azaltır:** Yaygın güvenlik işlemleri için tekrar eden kod yazmak yerine, bu kütüphane ile hızlı ve güvenilir çözümler sunar.
- **Best Practice Uyumlu:** Clean code prensiplerine uygun olarak geliştirilmiştir ve async yapı desteklidir.
- **Genişletilebilirlik:** Yeni güvenlik ihtiyaçlarına uygun şekilde genişletilebilir metotlar ve sınıflar içerir.

---

## **Avantajları**
- **Kolay Entegrasyon:** .NET projelerine kolayca entegre edilebilir.
- **Esnek Yapı:** Farklı senaryolara uygun metotlar içerir.
- **Performans:** Optimize edilmiş metotlar ile yüksek performans sunar.
- **Güvenilirlik:** Test edilmiş ve büyük ölçekli projelerde kullanıma uygun bir yapıya sahiptir.

---

## **Kurulum ve Entegrasyon**

### 2. **Program.cs ve AppSettings.json Ayarları**
Kütüphaneyi kullanmak için herhangi bir özel ayar gerekmemektedir. Ancak, özelleştirilmiş ayarlar yapmak isterseniz `appsettings.json` dosyasına ekleyebilirsiniz.

#### **Örnek `appsettings.json`:**
```json
{
  "SecuritySettings": {
    "DefaultMaskStart": 2,
    "DefaultMaskEnd": 2
  }
}
```

#### **Örnek `Program.cs` Entegrasyonu:**
```csharp
using Core.ToolKit.Security;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Kütüphane metotlarını kullanma örnekleri
app.MapGet("/mask", () =>
{
    var maskedEmail = MaskingHelper.Mask("example@domain.com", 3, 4);
    return maskedEmail; // Örnek çıktı: "exa****@domain.com"
});

app.MapGet("/encode", () =>
{
    var encodedText = TextSecurity.EncodeToBase64("Hello, World!");
    return encodedText; // Örnek çıktı: "SGVsbG8sIFdvcmxkIQ=="
});

app.Run();
```

---

## **Detaylı Kullanım Örnekleri**

### 1. **MaskingHelper Kullanımı**
#### **Metin Maskeleme:**
```csharp
var maskedEmail = MaskingHelper.Mask("example@domain.com", 3, 4);
// Çıktı: "exa****@domain.com"

var maskedPhone = MaskingHelper.Mask("+905551234567", 4, 3);
// Çıktı: "+905*****567"
```

#### **Rakamları Maskeleme:**
```csharp
var maskedDigits = MaskingHelper.MaskDigits("1234567890");
// Çıktı: "******7890"
```

---

### 2. **TextSecurity Kullanımı**
#### **Base64 Encoding:**
```csharp
var encodedText = TextSecurity.EncodeToBase64("Hello, World!");
// Çıktı: "SGVsbG8sIFdvcmxkIQ=="
```

#### **Base64 Decoding:**
```csharp
var decodedText = TextSecurity.DecodeFromBase64("SGVsbG8sIFdvcmxkIQ==");
// Çıktı: "Hello, World!"
```

---

### 3. **ValidationExtensions Kullanımı**
#### **Email Validasyonu:**
```csharp
var isValidEmail = "example@domain.com".IsValidEmail();
// Çıktı: true
```

#### **Telefon Numarası Validasyonu:**
```csharp
var isValidPhone = "+905551234567".IsValidPhoneNumber();
// Çıktı: true
```

#### **URL Validasyonu:**
```csharp
var isValidUrl = "https://www.example.com".IsValidUrl();
// Çıktı: true
```

---

### 4. **InputSanitizer Kullanımı**
#### **Input Sanitization:**
```csharp
var sanitizedInput = InputSanitizer.Sanitize("<script>alert('XSS')</script>");
// Çıktı: "scriptalertXSSscript"
```

#### **Alfanumerik Kontrol:**
```csharp
var isAlphanumeric = InputSanitizer.IsAlphanumeric("abc123");
// Çıktı: true
```

---

## **Gelişmiş Kullanım Senaryoları**

### 1. **Async Metotlar ile Kullanım**
Özellikle büyük veri setleri üzerinde çalışırken async metotlar kullanılabilir.

```csharp
public async Task<string> EncodeLargeDataAsync(string data)
{
    return await Task.Run(() => TextSecurity.EncodeToBase64(data));
}
```

### 2. **Özelleştirilmiş Masking Ayarları**
`appsettings.json` dosyasından maskeleme ayarlarını okuyabilirsiniz.

```csharp
var maskStart = builder.Configuration.GetValue<int>("SecuritySettings:DefaultMaskStart");
var maskEnd = builder.Configuration.GetValue<int>("SecuritySettings:DefaultMaskEnd");

var maskedText = MaskingHelper.Mask("example@domain.com", maskStart, maskEnd);
```

### 3. **Custom Regex ile Validasyon**
`ValidationExtensions` sınıfını genişleterek özel regex kuralları ekleyebilirsiniz.

```csharp
public static bool IsValidCustomFormat(this string input, string regexPattern)
{
    return Regex.IsMatch(input, regexPattern);
}
```

---

## **Sonuç**
`Core.ToolKit.Security`, .NET projelerinde güvenlik ihtiyaçlarını karşılamak için güçlü ve esnek bir kütüphanedir. Kolay entegrasyon, genişletilebilirlik ve best practice uyumu ile büyük ölçekli projelerde rahatlıkla kullanılabilir. Yukarıdaki örnekler ve entegrasyon adımları ile projenize hızlıca dahil edebilirsiniz.