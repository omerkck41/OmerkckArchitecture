# Core.Toolkit Security Module

**Security Module**, kullanıcı girişlerini temizleme, hassas bilgileri maskeleme ve çeşitli doğrulama işlemleri için güçlü araçlar sunar. Bu modül, projelerde güvenlik açıklarını azaltmaya ve verileri korumaya yönelik kapsamlı bir yapı sağlar.

---

## **1. InputSanitizer**

### **Açıklama**
Kullanıcı girişlerini temizler ve yalnızca izin verilen karakterlere izin verir.

### **Kullanım**

#### **Kullanıcı Girişini Temizleme**
```csharp
using Core.Toolkit.Security;

string sanitizedInput = InputSanitizer.Sanitize("<script>alert('Hello');</script>");
Console.WriteLine(sanitizedInput); // Output: scriptalert('Hello');script
```

#### **Alfanümerik Doğrulama**
```csharp
using Core.Toolkit.Security;

bool isValid = InputSanitizer.IsAlphanumeric("Hello123");
Console.WriteLine(isValid); // Output: True
```

#### **URL Doğrulama**
```csharp
using Core.Toolkit.Security;

bool isValidUrl = InputSanitizer.IsValidUrl("https://example.com");
Console.WriteLine(isValidUrl); // Output: True
```

---

## **2. TextSecurity**

### **Açıklama**
Metinleri Base64 formatında kodlama ve çözme işlemlerini gerçekleştirir.

### **Kullanım**

#### **Base64 Kodlama**
```csharp
using Core.Toolkit.Security;

string encodedText = TextSecurity.EncodeToBase64("Hello, World!");
Console.WriteLine(encodedText); // Output: SGVsbG8sIFdvcmxkIQ==
```

#### **Base64 Çözme**
```csharp
using Core.Toolkit.Security;

string decodedText = TextSecurity.DecodeFromBase64("SGVsbG8sIFdvcmxkIQ==");
Console.WriteLine(decodedText); // Output: Hello, World!
```

---

## **3. MaskingHelper**

### **Açıklama**
Hassas bilgilerin belirli bölümlerini maskelemek için kullanılır.

### **Kullanım**

#### **Metin Maskeleme**
```csharp
using Core.Toolkit.Security;

string maskedText = MaskingHelper.Mask("SensitiveData", 2, 2);
Console.WriteLine(maskedText); // Output: Se*********ta
```

#### **Rakam Maskeleme**
```csharp
using Core.Toolkit.Security;

string maskedDigits = MaskingHelper.MaskDigits("1234567890");
Console.WriteLine(maskedDigits); // Output: ******7890
```

---

## **4. ValidationExtensions**

### **Açıklama**
E-posta, telefon numarası ve URL gibi yaygın veri türlerini doğrulamak için uzantı metotları sağlar.

### **Kullanım**

#### **E-posta Doğrulama**
```csharp
using Core.Toolkit.Security;

bool isValidEmail = "user@example.com".IsValidEmail();
Console.WriteLine(isValidEmail); // Output: True
```

#### **Telefon Numarası Doğrulama**
```csharp
using Core.Toolkit.Security;

bool isValidPhone = "+1234567890".IsValidPhoneNumber();
Console.WriteLine(isValidPhone); // Output: True
```

#### **URL Doğrulama**
```csharp
using Core.Toolkit.Security;

bool isValidUrl = "https://example.com".IsValidUrl();
Console.WriteLine(isValidUrl); // Output: True
```

---

## **Özellikler ve Avantajlar**

- **Güçlü Giriş Temizleme**: Zararlı karakterleri güvenilir bir şekilde temizler.
- **Hassas Bilgi Koruması**: Kredi kartı ve telefon numarası gibi bilgileri maskeleyerek gizliliği artırır.
- **Kolay Doğrulama**: Veri türlerine göre hızlı ve doğru doğrulama sağlar.
- **Performans**: Hafif ve optimize edilmiş bir yapı.

---
