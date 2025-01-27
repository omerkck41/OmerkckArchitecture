### **README.md**

---

# ElasticSearch Entegrasyonu

Bu proje, .NET 9.0 mimarisi kullanılarak Elasticsearch ile entegre edilmiş bir uygulama örneğidir. Elasticsearch, büyük veri setleri üzerinde hızlı arama, analiz ve log yönetimi gibi işlemler için kullanılan güçlü bir arama motorudur. Bu projede, Elasticsearch ile temel CRUD (Create, Read, Update, Delete) işlemleri ve arama işlemleri gerçekleştirilmiştir.

---

## **ElasticSearch Nedir?**
Elasticsearch, dağıtılmış, RESTful bir arama ve analiz motorudur. Büyük miktarda veriyi gerçek zamanlı olarak depolamak, aramak ve analiz etmek için kullanılır. Özellikle log yönetimi, metin arama, veri analizi ve öneri sistemleri gibi alanlarda yaygın olarak kullanılır.

### **Ne İşe Yarar?**
- **Hızlı Arama:** Büyük veri setlerinde milisaniyeler içinde arama yapabilir.
- **Ölçeklenebilirlik:** Dağıtılmış yapısı sayesinde büyük veri setlerini kolayca yönetebilir.
- **Esnek Veri Modeli:** JSON tabanlı belgelerle çalışır, bu da farklı veri türlerini destekler.
- **Analiz ve Görselleştirme:** Logstash ve Kibana gibi araçlarla entegre çalışarak veri analizi ve görselleştirme sağlar.

---

## **Projede Kullanılan Sınıflar ve Yapılar**

### **1. ElasticSearchSettings**
Elasticsearch bağlantı bilgilerini tutan sınıftır. `ConnectionString`, `DefaultIndex`, `Username` ve `Password` gibi bilgileri içerir.

```csharp
public class ElasticSearchSettings
{
    public string ConnectionString { get; }
    public string DefaultIndex { get; }
    public string Username { get; }
    public string Password { get; }

    public ElasticSearchSettings(string connectionString, string defaultIndex, string username, string password)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        DefaultIndex = defaultIndex ?? throw new ArgumentNullException(nameof(defaultIndex));
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Password = password ?? throw new ArgumentNullException(nameof(password));
    }
}
```

### **2. IElasticSearchService**
Elasticsearch işlemlerini tanımlayan arayüzdür. CRUD işlemleri ve arama işlemleri bu arayüz üzerinden gerçekleştirilir.

```csharp
public interface IElasticSearchService
{
    Task<bool> CreateIndexAsync(string indexName, int numberOfShards, int numberOfReplicas);
    Task<bool> CreateIndexIfNotExistsAsync(string indexName, int numberOfShards, int numberOfReplicas);
    Task<bool> InsertDocumentAsync<T>(string indexName, string documentId, T document) where T : class;
    Task<bool> UpdateDocumentAsync<T>(string indexName, string documentId, T document) where T : class;
    Task<bool> DeleteDocumentAsync(string indexName, string documentId);
    Task<T?> GetDocumentByIdAsync<T>(string indexName, string documentId) where T : class;
    Task<List<T>> SearchDocumentsAsync<T>(string indexName, string query, int from, int size, string? sortField = null, bool isAscending = true) where T : class;
    Task<bool> BulkInsertAsync<T>(string indexName, List<T> documents) where T : class;
    Task<bool> DeleteIndexAsync(string indexName);
}
```

### **3. ElasticSearchService**
`IElasticSearchService` arayüzünü implemente eden sınıftır. Elasticsearch ile tüm işlemler bu sınıf üzerinden gerçekleştirilir.

```csharp
public class ElasticSearchService : IElasticSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticSearchService> _logger;

    public ElasticSearchService(IConfiguration configuration, ILogger<ElasticSearchService> logger)
    {
        _logger = logger;

        var settings = configuration.GetSection("ElasticSearchSettings").Get<ElasticSearchSettings>()
                      ?? throw new InvalidOperationException("ElasticSearch settings are not configured.");

        var clientSettings = new ElasticsearchClientSettings(new Uri(settings.ConnectionString))
            .DefaultIndex(settings.DefaultIndex)
            .Authentication(new BasicAuthentication(settings.Username, settings.Password));

        _client = new ElasticsearchClient(clientSettings);
    }

    // Diğer metodlar...
}
```

---

## **Program.cs ve AppSettings.json Ayarları**

### **1. AppSettings.json**
Elasticsearch bağlantı bilgileri `appsettings.json` dosyasında tanımlanır.

```json
{
  "ElasticSearchSettings": {
    "ConnectionString": "http://localhost:9200",
    "DefaultIndex": "my_default_index",
    "Username": "elastic",
    "Password": "your_password"
  }
}
```

### **2. Program.cs**
Elasticsearch servisi, `Program.cs` dosyasında DI (Dependency Injection) container'a eklenir.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Elasticsearch servisini ekle
builder.Services.AddElasticSearch(builder.Configuration);

var app = builder.Build();

// Diğer middleware'ler ve endpoint'ler...

app.Run();
```

---

## **Proje İçin Kullanım Örnekleri**

### **1. Index Oluşturma**
```csharp
var elasticSearchService = serviceProvider.GetRequiredService<IElasticSearchService>();
await elasticSearchService.CreateIndexAsync("my_index", 1, 1);
```

### **2. Belge Ekleme**
```csharp
var document = new { Name = "John Doe", Age = 30 };
await elasticSearchService.InsertDocumentAsync("my_index", "1", document);
```

### **3. Belge Güncelleme**
```csharp
var updatedDocument = new { Name = "Jane Doe", Age = 31 };
await elasticSearchService.UpdateDocumentAsync("my_index", "1", updatedDocument);
```

### **4. Belge Silme**
```csharp
await elasticSearchService.DeleteDocumentAsync("my_index", "1");
```

### **5. Belge Arama**
```csharp
var results = await elasticSearchService.SearchDocumentsAsync<MyDocumentType>("my_index", "John", 0, 10);
```

### **6. Toplu Belge Ekleme**
```csharp
var documents = new List<MyDocumentType>
{
    new MyDocumentType { Id = "1", Name = "John Doe", Age = 30 },
    new MyDocumentType { Id = "2", Name = "Jane Doe", Age = 31 }
};
await elasticSearchService.BulkInsertAsync("my_index", documents);
```

---

## **Bağımlılıklar**
- `.NET 9.0`
- `Elastic.Clients.Elasticsearch`
- `Elastic.Transport`
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Logging`

---

## **Kurulum ve Çalıştırma**
1. Elasticsearch'ü yerel makinenizde veya bir sunucuda çalıştırın.
2. `appsettings.json` dosyasındaki Elasticsearch bağlantı bilgilerini güncelleyin.
3. Projeyi derleyin ve çalıştırın.

---