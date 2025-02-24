
# Core.Persistence Library

**Core.Persistence**, Core Persistence Library, .NET Core projelerinde veri erişim katmanını (repository, unit of work) merkezi ve modüler bir şekilde yönetmek için tasarlanmış bir kütüphanedir. Ayrıca, soft delete/hard delete, dinamik filtreleme, sıralama ve paging (sayfalama) gibi işlevsellikleri de içerir. Bu kütüphane, karmaşık veri erişim operasyonlarını basitleştirerek, kod tekrarını azaltır ve uygulamanın bakımını kolaylaştırır.

---

## Neden Kullanılır?

- **Temiz Kod ve Modülerlik**:
Veri erişim mantığınızı merkezi repository ve unit of work desenleri ile yöneterek, kodunuzu daha okunabilir ve test edilebilir hale getirir.

- **Genişletilebilirlik**:
Soft delete/hard delete, cascade delete, dinamik filtreleme, sıralama ve paging gibi ek işlevsellikleri barındırır. Bu sayede, veri erişim ihtiyaçlarınızı esnek ve genişletilebilir bir yapı ile karşılayabilirsiniz.

- **Performans ve Güvenilirlik**:
Reflection cache, derlenmiş sorgular, global query filter gibi teknikler sayesinde yüksek performans ve tutarlı veri erişimi sağlar. Ayrıca, audit mekanizması ile değişiklikler izlenebilir ve hata yönetimi kolaylaşır.

---

## Avantajları

- **Merkezi Veri Erişimi:**
Tüm CRUD işlemlerini ve ek işlevsellikleri tek bir katmanda toplar.

- **Soft / Hard Delete Desteği:**
Her tablo için soft delete veya hard delete seçenekleri sunar; deletion türü, permanent parametresi ile kolayca yönetilir.

- **Dinamik Filtreleme ve Sıralama:**
Dinamik sorgu oluşturma desteği ile, runtime esnasında filtre ve sıralama kriterlerini belirleyebilirsiniz.

- **Paging (Sayfalama):**
IPaginate arayüzü ve ilgili extension metodları sayesinde, büyük veri setlerinde sayfalama işlemlerini kolayca gerçekleştirebilirsiniz.

- **Performans Optimizasyonları:**
Reflection delegate cacheleme, derlenmiş sorgular ve global query filter uygulamaları ile veri erişiminde verimlilik sağlar.

- **Test Edilebilirlik:**
Repository ve UnitOfWork desenlerinin kullanımı, birim testlerin kolayca yazılmasını sağlar. Ayrı bir test projesi ile kütüphanenizin tüm işlevselliği kapsamlı şekilde doğrulanabilir.

---

## Projeye Nasıl Eklenir?

### 1. **Proje Referansı Eklemek**

- **Core.Persistence** Eğer kütüphanenizi doğrudan kaynak kod olarak projeye dahil edecekseniz, aşağıdaki adımları izleyin:

- **1. Solution Explorer'da Sağ Tıklayın:**
Ana çözüm (Solution) üzerinde sağ tıklayıp "Add" > "Existing Project…" seçeneği ile kütüphane projesini ekleyin.

- **2. Ana Projeye Referans Ekleyin:**
Ana projenizin referanslarına sağ tıklayıp "Add Reference…" seçeneği ile kütüphane projesini seçin.

---

### Ayar Dosyaları Düzenleme

- program.cs Örneği
Program.cs dosyanızda kütüphanenizin servislerini DI container’a ekleyin. Örneğin:

```csharp
using Microsoft.EntityFrameworkCore;
using Core.Persistence.Repositories;
using Core.Persistence.UnitOfWork;
using Core.Persistence.Extensions;
using MyProject.Data; // DbContext'inizin bulunduğu namespace

var builder = WebApplication.CreateBuilder(args);

// DbContext ve InMemory/SQL Server gibi sağlayıcıların eklenmesi:
builder.Services.AddDbContext<MyAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository ve UnitOfWork desenlerini DI container'a ekleyin:
builder.Services.AddScoped(typeof(IAsyncRepository<,>), typeof(EfRepositoryBase<,,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork<MyAppDbContext>>();

// Diğer servislerin eklenmesi...
builder.Services.AddControllers();

var app = builder.Build();

// Global query filter (OnModelCreating içinde uygulandı)
// Uygulamanın diğer middleware ayarları...

app.MapControllers();

app.Run();
```
---

### appsettings.json Örneği
Bağlantı dizesi gibi ayarları appsettings.json içinde tanımlayabilirsiniz:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YourDatabase;User Id=YourUser;Password=YourPassword;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}

```
---

## Detaylı Kullanım Örnekleri

### 1. CRUD İşlemleri
Repository Kullanarak Ekleme:

```csharp
// Bir controller içinde örnek kullanım:
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IAsyncRepository<TestEntity, int> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TestController(IAsyncRepository<TestEntity, int> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<IActionResult> Create(TestEntity model)
    {
        var entity = await _repository.AddAsync(model);
        await _unitOfWork.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TestEntity model)
    {
        var entity = await _repository.GetByIdAsync(id);
        entity.Name = model.Name;
        var updatedEntity = await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return Ok(updatedEntity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] bool permanent = false)
    {
        var entity = await _repository.GetByIdAsync(id);
        await _repository.DeleteAsync(entity, permanent);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}

```

---

### 2. Dinamik Sorgu (Filtre ve Sıralama)
Dinamik sorguları uygulamak için Dynamic, Filter ve Sort sınıflarını kullanabilirsiniz:

```csharp
// Dinamik filtre ve sıralama örneği:
var filter = new Filter
{
    Field = "Name",
    Operator = "contains",
    Value = "Test"
};

var sorts = new List<Sort>
{
    new Sort { Field = "Name", Dir = "asc", Priority = 0 }
};

var dynamicQuery = new Dynamic(sorts, filter);

// IQueryable üzerinde dinamik sorgu uygulanması:
var filteredData = await _repository.GetListByDynamicAsync(dynamicQuery, index: 1, size: 10);

```

---

### 3. Sayfalama (Paging)
Paging işlemleri için IPaginate arayüzü ve extension metodları kullanılmaktadır:

```csharp
// Sayfalama örneği:
var pageRequest = new PageRequest { Page = 1, PageSize = 5 };
var pagedData = await _repository.GetListAsync(index: pageRequest.Page, size: pageRequest.PageSize);

```
---

## Test Projesi

Kütüphanenizin işlevselliğini doğrulamak için ayrı bir test projesi oluşturmanız önerilir. Bu test projesi, yukarıda anlatılan CRUD, dinamik filtre, sıralama, paging gibi tüm senaryoları kapsamlı şekilde test eden bir test modülünü içerecektir. Test projesi eklemek için Visual Studio'da yeni bir xUnit Test Project oluşturun, ana kütüphanenize referans ekleyin ve yukarıdaki test örneklerini kullanarak testlerinizi yazın.

## Sonuç

Core Persistence Library, veri erişim katmanınızı merkezi, modüler ve genişletilebilir hale getirirken; soft/hard delete, dinamik sorgulama, sıralama ve paging gibi gelişmiş özellikler sunar. Bu README dosyası ile kütüphanenin ne olduğu, neden kullanıldığı, avantajları, projeye nasıl entegre edileceği ve kullanım örnekleri detaylı şekilde açıklanmıştır.

---