# Core.ToolKit.DataProcessing

**Core.ToolKit.DataProcessing**, veri işleme, dönüşüm, maskeleme ve şifreleme gibi işlemleri kolaylaştırmak için geliştirilmiş bir .NET kütüphanesidir. Bu kütüphane, büyük ölçekli projelerde veri işleme ihtiyaçlarını karşılamak üzere tasarlanmıştır ve `DataTable`, `JSON`, `CSV` gibi yaygın veri formatlarıyla çalışmayı kolaylaştırır. Ayrıca, hassas verilerin maskelemesi ve şifrelenmesi için güçlü araçlar sunar.

---

## **Nedir?**

**Core.ToolKit.DataProcessing**, aşağıdaki işlemleri gerçekleştirmek için kullanılan bir araç setidir:

- **DataTable** ve nesne listeleri arasında dönüşüm yapma.
- **JSON** dosyalarını okuma ve yazma.
- **CSV** dosyalarını okuma ve yazma.
- Hassas verileri maskeleme (örneğin, e-posta, telefon numarası).
- Verileri şifreleme ve şifre çözme (AES şifreleme).

Bu kütüphane, temiz kod prensiplerine uygun olarak geliştirilmiştir ve asenkron programlamayı destekler.

---

## **Neden Kullanılır?**

- **Veri Dönüşümü**: `DataTable`, `JSON`, `CSV` gibi formatlar arasında kolayca dönüşüm yapmak için kullanılır.
- **Veri Güvenliği**: Hassas verilerin maskelemesi ve şifrelenmesi için güçlü araçlar sunar.
- **Performans**: Büyük veri setleriyle çalışırken performans optimizasyonları sağlar.
- **Esneklik**: Farklı veri formatları ve senaryolar için genişletilebilir bir yapı sunar.

---

## **Avantajları**

- **Kolay Kullanım**: Basit ve anlaşılır metotlar ile hızlı bir şekilde entegre edilebilir.
- **Genişletilebilirlik**: Yeni veri formatları ve işlemler için kolayca genişletilebilir.
- **Güvenlik**: Hassas verilerin güvenli bir şekilde işlenmesini sağlar.
- **Asenkron Destek**: Tüm metotlar asenkron programlamayı destekler, böylece büyük veri setleriyle çalışırken performans kaybı yaşanmaz.

---

## **Kurulum ve Projeye Ekleme**

### 1. **NuGet Paketi Olarak Ekleme**
Kütüphaneyi NuGet üzerinden projenize ekleyebilirsiniz:

```bash
dotnet add package Core.ToolKit.DataProcessing
```

### 2. **Manuel Olarak Ekleme**
Eğer NuGet kullanmıyorsanız, `Core.ToolKit.DataProcessing` klasörünü projenize ekleyebilir ve referans olarak gösterebilirsiniz.

---

## **Ayarlar ve Yapılandırma**

### **Program.cs**
Kütüphaneyi kullanmak için `Program.cs` dosyasında herhangi bir özel ayar yapmanıza gerek yoktur. Ancak, asenkron metotları kullanırken `async/await` yapısını kullanmanız önerilir.

```csharp
using Core.ToolKit.DataProcessing;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Örnek kullanım
        var dataTable = new DataTable();
        var list = DataTableHelper.ToList<MyModel>(dataTable);

        var jsonData = await JsonHelper.ReadJsonAsync<MyModel>("data.json");
        await JsonHelper.WriteJsonAsync(jsonData, "output.json");

        var csvData = await CsvHelper.ReadCsvAsync("data.csv");
        await CsvHelper.WriteCsvAsync(csvData, "output.csv");

        var maskedEmail = DataMasker.MaskEmail("example@domain.com");
        var encryptedData = DataMasker.Encrypt("SensitiveData", "MySecretKey");
    }
}
```

### **appsettings.json**
Kütüphane, `appsettings.json` dosyasından herhangi bir ayar okumaz. Ancak, şifreleme anahtarı gibi hassas bilgileri `appsettings.json` dosyasında saklayabilir ve bu bilgileri program içinde kullanabilirsiniz.

```json
{
  "EncryptionSettings": {
    "Key": "MySecretKey"
  }
}
```

---

## **Detaylı Kullanım Örnekleri**

### 1. **DataTableHelper Kullanımı**

#### **DataTable'dan Liste Oluşturma**
```csharp
var dataTable = new DataTable();
// DataTable'ı doldurma işlemleri...

var list = DataTableHelper.ToList<MyModel>(dataTable);
```

#### **Listeden DataTable Oluşturma**
```csharp
var myList = new List<MyModel>
{
    new MyModel { Id = 1, Name = "John" },
    new MyModel { Id = 2, Name = "Jane" }
};

var dataTable = DataTableHelper.ToDataTable(myList);
```

---

### 2. **JsonHelper Kullanımı**

#### **JSON Dosyasını Okuma**
```csharp
var jsonData = await JsonHelper.ReadJsonAsync<MyModel>("data.json");
```

#### **JSON Dosyasına Yazma**
```csharp
var myData = new MyModel { Id = 1, Name = "John" };
await JsonHelper.WriteJsonAsync(myData, "output.json", indented: true);
```

---

### 3. **CsvHelper Kullanımı**

#### **CSV Dosyasını Okuma**
```csharp
var csvData = await CsvHelper.ReadCsvAsync("data.csv");
```

#### **CSV Dosyasına Yazma**
```csharp
var myList = new List<MyModel>
{
    new MyModel { Id = 1, Name = "John" },
    new MyModel { Id = 2, Name = "Jane" }
};

await CsvHelper.WriteCsvAsync(myList, "output.csv");
```

---

### 4. **DataMasker Kullanımı**

#### **E-posta Maskeleme**
```csharp
var maskedEmail = DataMasker.MaskEmail("example@domain.com");
// Sonuç: ex****@domain.com
```

#### **Telefon Numarası Maskeleme**
```csharp
var maskedPhone = DataMasker.MaskPhoneNumber("1234567890");
// Sonuç: ******7890
```

#### **Veri Şifreleme ve Çözme**
```csharp
var encryptedData = DataMasker.Encrypt("SensitiveData", "MySecretKey");
var decryptedData = DataMasker.Decrypt(encryptedData, "MySecretKey");
```

---

## **Sonuç**

**Core.ToolKit.DataProcessing**, veri işleme, dönüşüm, maskeleme ve şifreleme işlemlerini kolaylaştıran güçlü bir kütüphanedir. Büyük ölçekli projelerde rahatça kullanılabilir ve temiz kod prensiplerine uygun olarak geliştirilmiştir. Bu kütüphane ile veri işleme süreçlerinizi hızlandırabilir ve güvenli bir şekilde yönetebilirsiniz.