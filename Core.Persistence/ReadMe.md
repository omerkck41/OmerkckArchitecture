Aşağıda, projeniz için detaylı bir **README.md** dosyası örneği bulabilirsiniz. Bu dosya, projenin mimarisini, nasıl kurulacağını, nasıl kullanılacağını ve örnek metotları içermektedir.

---

# Core.Persistence Library

**Core.Persistence**, .NET projelerinde kullanılmak üzere geliştirilmiş bir persistence (kalıcılık) katmanıdır. Bu kütüphane, generic repository pattern, unit of work pattern, dinamik sorgulama, sayfalama ve soft delete gibi özellikleri destekler. Entity Framework Core ile entegre çalışır ve .NET 9.0 üzerinde geliştirilmiştir.

---

## Teknolojiler ve Mimari

### Kullanılan Teknolojiler
- **.NET 9.0**: Proje, .NET 9.0 üzerinde geliştirilmiştir.
- **Entity Framework Core**: Veritabanı işlemleri için EF Core kullanılmıştır.
- **Generic Repository Pattern**: Veritabanı işlemleri için genel bir repository yapısı sunar.
- **Unit of Work Pattern**: Transaction yönetimi ve repository'lerin yaşam döngüsünü yönetir.
- **Dynamic Query**: Dinamik filtreleme ve sıralama işlemlerini destekler.
- **Pagination**: Sayfalama işlemleri için güçlü bir yapı sunar.
- **Soft Delete**: Veritabanından silinen kayıtları fiziksel olarak silmek yerine işaretleyerek saklar.

---

## Projeye Nasıl Eklenir?

### 1. **Projeye Ekleme**
- **Core.Persistence** kütüphanesini projenize eklemek için, proje dosyanıza (`csproj`) aşağıdaki paket referanslarını ekleyin:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.1" />
</ItemGroup>
```

- **Core.Persistence** katmanını projenize referans olarak ekleyin.

---

### 2. **Program.cs Ayarları**
- **Program.cs** dosyasında, Entity Framework Core ve UnitOfWork yapılandırmasını yapın:

```csharp
var builder = WebApplication.CreateBuilder(args);

// DbContext ve UnitOfWork yapılandırması
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork<YourDbContext>>();

var app = builder.Build();
```

---

### 3. **appsettings.json Ayarları**
- **appsettings.json** dosyasında, veritabanı bağlantı dizesini ekleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=your_database;User Id=your_user;Password=your_password;"
  }
}
```

---

## Nasıl Kullanılır?

### 1. **Repository ve UnitOfWork Kullanımı**
- **UnitOfWork** ve **Repository** yapısını kullanarak veritabanı işlemlerini gerçekleştirebilirsiniz.

```csharp
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        var repository = _unitOfWork.Repository<Product, int>();
        return await repository.GetByIdAsync(id);
    }

    public async Task AddProductAsync(Product product)
    {
        var repository = _unitOfWork.Repository<Product, int>();
        await repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

---

### 2. **Dinamik Sorgulama ve Sayfalama**
- **Dynamic** sınıfı ile dinamik filtreleme ve sıralama işlemleri yapabilirsiniz.

```csharp
public async Task<IPaginate<Product>> GetProductsAsync(string filterField, string filterValue, string sortField, string sortDir)
{
    var repository = _unitOfWork.Repository<Product, int>();

    var dynamic = new Dynamic(
        sort: new List<Sort> { new Sort { Field = sortField, Dir = sortDir } },
        filter: new Filter { Field = filterField, Operator = "eq", Value = filterValue }
    );

    return await repository.GetListByDynamicAsync(dynamic);
}
```

---

### 3. **Soft Delete İşlemleri**
- **SoftDeleteAsync** metodu ile kayıtları fiziksel olarak silmek yerine işaretleyebilirsiniz.

```csharp
public async Task SoftDeleteProductAsync(int id)
{
    var repository = _unitOfWork.Repository<Product, int>();
    var product = await repository.GetByIdAsync(id);

    if (product != null)
    {
        await repository.SoftDeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

---

### 4. **Toplu Güncelleme İşlemleri**
- **BulkUpdateAsync** metodu ile toplu güncelleme işlemleri yapabilirsiniz.

```csharp
public async Task BulkUpdateProductPricesAsync(decimal newPrice)
{
    var repository = _unitOfWork.Repository<Product, int>();

    await repository.BulkUpdateAsync(
        predicate: p => p.Price < newPrice,
        updates: new[] { (p => p.Price, newPrice) }
    );

    await _unitOfWork.SaveChangesAsync();
}
```

---

### 5. **Sayfalama İşlemleri**
- **ToPaginateAsync** metodu ile sayfalama işlemleri yapabilirsiniz.

```csharp
public async Task<IPaginate<Product>> GetProductsPaginatedAsync(int pageIndex, int pageSize)
{
    var repository = _unitOfWork.Repository<Product, int>();
    return await repository.GetListAsync(index: pageIndex, size: pageSize);
}
```

---

## Metot Örnekleri

### 1. **GetByIdAsync**
```csharp
var product = await repository.GetByIdAsync(1);
```

### 2. **AddAsync**
```csharp
var product = new Product { Name = "New Product", Price = 100 };
await repository.AddAsync(product);
await _unitOfWork.SaveChangesAsync();
```

### 3. **UpdateAsync**
```csharp
var product = await repository.GetByIdAsync(1);
product.Price = 200;
await repository.UpdateAsync(product);
await _unitOfWork.SaveChangesAsync();
```

### 4. **DeleteAsync**
```csharp
var product = await repository.GetByIdAsync(1);
await repository.DeleteAsync(product);
await _unitOfWork.SaveChangesAsync();
```

### 5. **SoftDeleteAsync**
```csharp
var product = await repository.GetByIdAsync(1);
await repository.SoftDeleteAsync(product, deletedBy: "Admin");
await _unitOfWork.SaveChangesAsync();
```

### 6. **GetListByDynamicAsync**
```csharp
var dynamic = new Dynamic(
    sort: new List<Sort> { new Sort { Field = "Name", Dir = "asc" } },
    filter: new Filter { Field = "Price", Operator = "gt", Value = 100 }
);

var products = await repository.GetListByDynamicAsync(dynamic);
```

---