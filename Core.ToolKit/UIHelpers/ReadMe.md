# Core.Toolkit UI Helpers Module

**UI Helpers Module**, standartlaştırılmış bildirim mesajları ve hata sayfaları oluşturmak için güçlü araçlar sağlar. Modül, kullanıcıya anlamlı geri bildirimler sunarken geliştiricilere kolay entegrasyon sağlar.

---

## **1. Bildirim Mesajları (NotificationHelper)**

### **Açıklama**
Başarı, hata veya uyarı mesajlarını standart bir formatta oluşturur ve gerektiğinde konsola loglar.

### **Kullanım**

#### **Başarı Mesajı Oluşturma**
```csharp
using Core.Toolkit.UIHelpers;

string successMessage = NotificationHelper.Success("Operation completed successfully.", "The process took 3 seconds.");
Console.WriteLine(successMessage);
```

#### **Hata Mesajı Oluşturma**
```csharp
using Core.Toolkit.UIHelpers;

string errorMessage = NotificationHelper.Error("An error occurred.", "Stack trace: ...");
Console.WriteLine(errorMessage);
```

#### **Uyarı Mesajı Oluşturma**
```csharp
using Core.Toolkit.UIHelpers;

string warningMessage = NotificationHelper.Warning("This is a warning.", "You might encounter issues if you proceed.");
Console.WriteLine(warningMessage);
```

#### **Mesajı Konsola Loglama**
```csharp
using Core.Toolkit.UIHelpers;

NotificationHelper.Log("System started successfully.");
```

---

## **2. Hata Sayfası Oluşturma (ErrorPageGenerator)**

### **Açıklama**
HTTP hata kodları ve mesajları için basit HTML sayfaları oluşturur. 

### **Kullanım**

#### **Hata Sayfası Oluşturma**
```csharp
using Core.Toolkit.UIHelpers;

string errorPage = ErrorPageGenerator.Generate(404, "The page you are looking for does not exist.");
Console.WriteLine(errorPage);
```

#### **HTML Sayfasını Dosyaya Kaydetme**
```csharp
using Core.Toolkit.UIHelpers;
using System.IO;

string errorPage = ErrorPageGenerator.Generate(500, "Internal server error.");
File.WriteAllText("error500.html", errorPage);
```

---

## **Özellikler ve Avantajlar**

- **Standartlaştırılmış Mesajlar**: Başarı, hata ve uyarılar için tutarlı format sağlar.
- **Hata Sayfaları**: Basit ama etkili HTML hata sayfaları oluşturur.
- **Esnek ve Kullanıcı Dostu**: Opsiyonel detaylarla özelleştirilebilir.
- **Performans**: Hafif ve yüksek performanslıdır.

---

## **Pratik Örnekler**

### **Örnek 1: Kullanıcıya Başarı Mesajı Gösterme**
```csharp
string message = NotificationHelper.Success("User registration completed successfully.");
NotificationHelper.Log(message);
```

### **Örnek 2: Dinamik Hata Sayfası Oluşturma**
```csharp
string htmlContent = ErrorPageGenerator.Generate(403, "You do not have permission to access this page.");
File.WriteAllText("403.html", htmlContent);
```

---