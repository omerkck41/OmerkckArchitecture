# Core.Application.ElasticSearch - Kullanım ve Entegrasyon Rehberi

## **Giriş**
Core.Application.ElasticSearch modülü, ElasticSearch üzerinde temel CRUD, arama ve index yönetimi işlemleri yapmak için tasarlanmıştır. Bu modül, esnek ve genişletilebilir yapısıyla projelere kolayca entegre edilebilir.

---

## **1. Şema**
```
Core.Application.ElasticSearch
├── ElasticSearchSettings.cs
├── Interfaces
│   └── IElasticSearchService.cs
├── Services
│   └── ElasticSearchService.cs
```

---

## **2. Projeye Entegrasyon**

### **Adım 1: NuGet Paketlerinin Kurulumu**
Core.Application.ElasticSearch modülünü kullanabilmek için ElasticSearch kütüphanesini yükleyin:

```bash
Install-Package Elastic.Clients.Elasticsearch
```

### **Adım 2: Proje Referansı Ekleme**
ElasticSearch modülünü mevcut projeye referans eklemek için aşağıdaki komutu kullanabilirsiniz:

```bash
dotnet add reference Core.Application.ElasticSearch/Core.Application.ElasticSearch.csproj
```

### **Adım 3: Program.cs Yapılandırması**
`Program.cs` dosyasına ElasticSearch servisini ekleyin:

```csharp
using Core.Application.ElasticSearch;

var builder = WebApplication.CreateBuilder(args);

// ElasticSearch Ayarları
builder.Services.Configure<ElasticSearchSettings>(builder.Configuration.GetSection("ElasticSearchSettings"));

// ElasticSearch Servisi
builder.Services.AddSingleton<IElasticSearchService, ElasticSearchService>();

var app = builder.Build();
app.Run();
```

---

### **Adım 4: appsettings.json Yapılandırması**
`appsettings.json` dosyasına ElasticSearch bağlantı ayarlarını ekleyin:

```json
{
  "ElasticSearchSettings": {
    "ConnectionString": "http://localhost:9200",
    "DefaultIndex": "default-index"
  }
}
```

- **ConnectionString**: ElasticSearch sunucusunun URL'si.
- **DefaultIndex**: Varsayılan index adı.

---

## **3. Kullanım Örnekleri**

### **3.1. Index Oluşturma**
Yeni bir index oluşturmak için:
```csharp
var elasticSearchService = app.Services.GetRequiredService<IElasticSearchService>();
bool isCreated = await elasticSearchService.CreateIndexAsync("products", 3, 2);
Console.WriteLine(isCreated ? "Index created successfully" : "Index creation failed");
```

---

### **3.2. Belge Ekleme**
Index'e yeni bir belge eklemek için:
```csharp
var product = new { Id = 1, Name = "Laptop", Price = 1500 };
bool isInserted = await elasticSearchService.InsertDocumentAsync("products", "1", product);
Console.WriteLine(isInserted ? "Document inserted successfully" : "Document insertion failed");
```

---

### **3.3. Belge Güncelleme**
Var olan bir belgeyi güncellemek için:
```csharp
var updatedProduct = new { Id = 1, Name = "Gaming Laptop", Price = 2000 };
bool isUpdated = await elasticSearchService.UpdateDocumentAsync("products", "1", updatedProduct);
Console.WriteLine(isUpdated ? "Document updated successfully" : "Document update failed");
```

---

### **3.4. Belge Silme**
Bir belgeyi silmek için:
```csharp
bool isDeleted = await elasticSearchService.DeleteDocumentAsync("products", "1");
Console.WriteLine(isDeleted ? "Document deleted successfully" : "Document deletion failed");
```

---

### **3.5. Belge Arama**
Bir index üzerinde arama yapmak için:
```csharp
var results = await elasticSearchService.SearchDocumentsAsync<dynamic>("products", "Laptop", 0, 10);
foreach (var item in results)
{
    Console.WriteLine(item);
}
```

---

### **3.6. Toplu Belge Ekleme**
Birden fazla belgeyi aynı anda eklemek için:
```csharp
var products = new List<dynamic>
{
    new { Id = 2, Name = "Desktop", Price = 1200 },
    new { Id = 3, Name = "Tablet", Price = 500 }
};
bool isBulkInserted = await elasticSearchService.BulkInsertAsync("products", products);
Console.WriteLine(isBulkInserted ? "Bulk insert successful" : "Bulk insert failed");
```

---

### **3.7. Index Silme**
Bir index'i tamamen silmek için:
```csharp
bool isIndexDeleted = await elasticSearchService.DeleteIndexAsync("products");
Console.WriteLine(isIndexDeleted ? "Index deleted successfully" : "Index deletion failed");
```

---

## **4. Avantajları**

1. **Modern API Kullanımı**: Yeni kütüphane, temiz ve modern bir API sunar.
2. **Yüksek Performans**: ElasticSearch modülü, büyük veri setleri üzerinde hızlı sorgular çalıştırır.
3. **Esnek Arama**: Full-text arama, filtreleme ve query-based arama desteklenir.
4. **Modüler Yapı**: CRUD işlemleri, arama ve index yönetimi için ayrı ayrı kullanılabilir.
5. **Kolay Entegrasyon**: Program.cs ve appsettings.json yapılandırmalarıyla hızlı kurulum.

---

## **Sonuç**
Core.Application.ElasticSearch modülü, büyük projelerde veri arama ve yönetimi için güçlü bir çözüm sunar.
Modüler yapısı sayesinde kolayca genişletilebilir ve özelleştirilebilir.
ElasticSearch'in hız ve esnekliğini kullanarak projelerinizde arama, analiz ve log yönetimi gibi kritik ihtiyaçları karşılayabilirsiniz.
