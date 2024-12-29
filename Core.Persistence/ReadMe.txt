3.1. Connection String
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TestDatabase;Trusted_Connection=True;"
  }
}

3.2. Program.cs Yapılandırması
// DbContext'leri ekleyin
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Generic Repository ve UnitOfWork servislerini ekleyin
builder.Services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepositoryBase<,>));
builder.Services.AddScoped<IUnitOfWork<ApplicationDbContext>, UnitOfWork<ApplicationDbContext>>();

4. Dependency Injection (DI) ile Entegrasyon
private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;

public PersonelController(IUnitOfWork<ApplicationDbContext> unitOfWork)
{
   _unitOfWork = unitOfWork;
}

5.1. CRUD İşlemleri
await _unitOfWork.Repository<Personel>().AddAsync(new Personel { Name = "Ali", Age = 30 });
await _unitOfWork.SaveChangesAsync();

var personel = await _unitOfWork.Repository<Personel>().GetAsync(p => p.Id == 1);
await _unitOfWork.Repository<Personel>().DeleteAsync(personel);
await _unitOfWork.SaveChangesAsync();

var personel = await _unitOfWork.Repository<Personel>().GetAsync(p => p.Id == 1);
personel.Name = "Updated Name";
await _unitOfWork.Repository<Personel>().UpdateAsync(personel);
await _unitOfWork.SaveChangesAsync();

var personel = new Personel { Id = 5 }; // Yalnızca Id belirtilir
await _unitOfWork.Repository<Personel>().UpdatePartialAsync(
    personel,
    (p => p.xxx1, true),
    (p => p.xxx2, "Value")
);
await _unitOfWork.SaveChangesAsync()

5.2. Paging (Sayfalama)
var pagedList = await _unitOfWork.Repository<Personel>().GetListAsync(index: 0, size: 10);
return Ok(pagedList);

5.3. Dynamic Filtreleme ve Sıralama
var filter = new Filter
{
    Field = "Age",
    Operator = "gte",
    Value = "30"
};
var dynamicQuery = new Dynamic { Filter = filter };
var result = await _unitOfWork.Repository<Personel>().GetListByDynamicAsync(dynamicQuery);
return Ok(result);

var sort = new Sort
{
    Field = "Name",
    Dir = "asc"
};
var dynamicQuery = new Dynamic { Sort = new[] { sort } };
var result = await _unitOfWork.Repository<Personel>().GetListByDynamicAsync(dynamicQuery);
return Ok(result);


6. Çoklu DbContext Kullanımı
builder.Services.AddDbContext<OtherDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OtherConnection")));

builder.Services.AddScoped<IUnitOfWork<OtherDbContext>, UnitOfWork<OtherDbContext>>();

Controller Kullanımı
private readonly IUnitOfWork<ApplicationDbContext> _appUnitOfWork;
private readonly IUnitOfWork<OtherDbContext> _otherUnitOfWork;

    public MultiContextController(
        IUnitOfWork<ApplicationDbContext> appUnitOfWork,
        IUnitOfWork<OtherDbContext> otherUnitOfWork)
    {
        _appUnitOfWork = appUnitOfWork;
        _otherUnitOfWork = otherUnitOfWork;
    }

    [HttpPost("cross-db")]
    public async Task<IActionResult> AddToBothDbContexts()
    {
        await _appUnitOfWork.Repository<Personel>().AddAsync(new Personel { Name = "From AppDb" });
        await _otherUnitOfWork.Repository<OtherEntity>().AddAsync(new OtherEntity { Title = "From OtherDb" });

        await _appUnitOfWork.SaveChangesAsync();
        await _otherUnitOfWork.SaveChangesAsync();

        return Ok("İşlemler tamamlandı.");
    }