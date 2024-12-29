# Core.Persistence - README

**Core.Persistence**, modern yazılım geliştirme ihtiyaçlarını karşılamak için tasarlanmış, esnek, ölçeklenebilir ve güçlü bir veri erişim kütüphanesidir. Repository ve UnitOfWork tasarım desenlerini destekler, dinamik sorgular, sayfalama ve soft delete gibi özellikler sunar.

Bu kılavuz, Core.Persistence kütüphanesini projelere nasıl entegre edeceğinizi ve kullanacağınızı adım adım açıklamaktadır.

---

## **1. Özellikler**

1. **Repository Pattern**: Veri erişimini soyutlayarak düzenli ve temiz bir mimari sunar.
2. **UnitOfWork**: Transaction yönetimini kolaylaştırır.
3. **Dinamik Sorgular**: Dinamik filtreleme ve sıralama desteği.
4. **Sayfalama**: Büyük veri kümelerinde performanslı sayfalama.
5. **Soft Delete**: Silinen kayıtların fiziksel olarak değil, mantıksal olarak silinmesi.
6. **Multi-DbContext Desteği**: Birden fazla veritabanı bağlamını yönetebilme.

---

## **2. Projeye Entegrasyon**

### 2.1 NuGet Paketleri

Core.Persistence ile çalışmak için aşağıdaki NuGet paketlerini yüklemeniz gereklidir:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Tools`
- `System.Linq.Dynamic.Core`

```bash
Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package System.Linq.Dynamic.Core
```

---

### 2.2 Proje Yapılandırması

#### DbContext Tanımı

Projede kullanılacak DbContext sınıfını oluşturun:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("YourConnectionString");
    }
}
```

#### Program.cs

DI (Dependency Injection) ile repository ve UnitOfWork yapılarını sisteme dahil edin:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IAsyncRepository<,>), typeof(EfRepositoryBase<,,>));
builder.Services.AddScoped<IUnitOfWork<AppDbContext>, UnitOfWork<AppDbContext>>();
```

---

## **3. Kullanım**

### 3.1 Repository Kullanımı

**Repository**, veri erişim işlemlerini soyutlamak için kullanılır. Aşağıda bazı örnekler verilmiştir.

#### 3.1.1 Tekil Veri Erişimi

```csharp
var user = await _userRepository.GetAsync(u => u.Id == 1);
```

#### 3.1.2 Listeleme ve Sayfalama

```csharp
var users = await _userRepository.GetListAsync(
    predicate: u => !u.IsDeleted,
    orderBy: q => q.OrderBy(u => u.Name),
    index: 0,
    size: 10);
```

#### 3.1.3 Dinamik Sorgular

```csharp
var dynamic = new Dynamic
{
    Filter = new Filter("Name", "contains", "John"),
    Sort = new List<Sort> { new Sort("Age", "desc") }
};

var users = await _userRepository.GetListByDynamicAsync(dynamic);
```

#### 3.1.4 Veri Ekleme

```csharp
var newUser = new User { Name = "Jane Doe", Age = 30 };
await _userRepository.AddAsync(newUser);
await _unitOfWork.SaveChangesAsync();
```

#### 3.1.5 Güncelleme

```csharp
var user = await _userRepository.GetAsync(u => u.Id == 1);
user.Name = "Updated Name";
await _userRepository.UpdateAsync(user);
await _unitOfWork.SaveChangesAsync();
```

#### 3.1.6 Soft Delete

```csharp
await _userRepository.SoftDeleteAsync(1);
await _unitOfWork.SaveChangesAsync();
```

---

### 3.2 UnitOfWork Kullanımı

**UnitOfWork**, birden fazla repository ile çalışırken transaction yönetimini kolaylaştırır.

#### Örnek Kullanım:

```csharp
using (var unitOfWork = _unitOfWork.BeginTransactionAsync())
{
    var user = new User { Name = "New User" };
    await _userRepository.AddAsync(user);

    var anotherUser = new User { Name = "Another User" };
    await _userRepository.AddAsync(anotherUser);

    await _unitOfWork.SaveChangesAsync();
    await unitOfWork.CommitAsync();
}
```

---

### 3.3 Dinamik Filtreleme ve Sıralama

Dinamik sorgular, kullanıcıdan gelen kriterlere göre sorgular oluşturmanıza olanak tanır.

#### Örnek:

```csharp
var dynamic = new Dynamic
{
    Filter = new Filter("Name", "eq", "John"),
    Sort = new List<Sort> { new Sort("Age", "asc") }
};

var result = await _userRepository.GetListByDynamicAsync(dynamic);
```

---

### 3.4 Çoklu DbContext Desteği

```csharp
builder.Services.AddDbContext<OtherDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OtherConnection")));

builder.Services.AddScoped<IUnitOfWork<OtherDbContext>, UnitOfWork<OtherDbContext>>();

// Kullanım:
private readonly IUnitOfWork<AppDbContext> _appUnitOfWork;
private readonly IUnitOfWork<OtherDbContext> _otherUnitOfWork;

public MultiContextController(
    IUnitOfWork<AppDbContext> appUnitOfWork,
    IUnitOfWork<OtherDbContext> otherUnitOfWork)
{
    _appUnitOfWork = appUnitOfWork;
    _otherUnitOfWork = otherUnitOfWork;
}

[HttpPost("cross-db")]
public async Task<IActionResult> AddToBothDbContexts()
{
    await _appUnitOfWork.Repository<User>().AddAsync(new User { Name = "From AppDb" });
    await _otherUnitOfWork.Repository<OtherEntity>().AddAsync(new OtherEntity { Title = "From OtherDb" });

    await _appUnitOfWork.SaveChangesAsync();
    await _otherUnitOfWork.SaveChangesAsync();

    return Ok("İşlemler tamamlandı.");
}
```

---

## **4. Geliştirme İpuçları**

- **Performans**: Gereksiz sorgu çalıştırmamak için yalnızca gerekli alanları sorgulayan filtreleri kullanın.
- **Test Edilebilirlik**: Mock DbContext kullanarak repositorylerinizi kolayca test edebilirsiniz.
- **Genişletilebilirlik**: Gereksinimlerinize göre `IAsyncRepository` ve `EfRepositoryBase` sınıflarını genişletebilirsiniz.

---

## **5. Sonuç**

Core.Persistence, veri erişim operasyonlarınızı düzenlemek, performansı artırmak ve kodunuzu daha temiz hale getirmek için tasarlanmıştır.
Bu rehberde yer alan örnekler ve açıklamalarla, kütüphaneyi projelerinize kolayca entegre edebilir ve kullanmaya başlayabilirsiniz.
