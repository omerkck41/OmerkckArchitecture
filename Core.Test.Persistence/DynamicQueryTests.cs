using Core.Persistence.Dynamic;
using Microsoft.EntityFrameworkCore;

namespace TestPersistence;

public class DynamicQueryTests : IDisposable
{
    private readonly TestDbContext _context;

    public DynamicQueryTests()
    {
        // Her test için benzersiz bir InMemory veritabanı oluşturuluyor.
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);

        // Test verisini ekleyelim.
        _context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" },
            new TestEntity { Id = 3, Name = "Charlie" }
        );
        _context.SaveChanges();
    }

    [Fact]
    public void DynamicFilter_ShouldReturnCorrectRecord()
    {
        // Arrange: "Name eq Bob" filtresi tanımlanıyor.
        var filter = new Filter
        {
            Field = "Name",
            Operator = "eq",
            Value = "Bob"
        };
        var dynamicQuery = new Dynamic(null, filter);

        // Act: Dinamik filtre uygulanıyor.
        var query = _context.TestEntities.AsQueryable().ToDynamic(dynamicQuery);
        var result = query.ToList();

        // Assert: Sadece "Bob" kaydı döndürülmeli.
        Assert.Single(result);
        Assert.Equal("Bob", result.First().Name);
    }

    [Fact]
    public void DynamicSort_ShouldReturnRecordsInCorrectOrder()
    {
        // Arrange: "Name" alanına göre azalan sıralama tanımlanıyor.
        var sorts = new List<Sort>
            {
                new Sort { Field = "Name", Dir = "desc", Priority = 0 }
            };
        var dynamicQuery = new Dynamic(sorts, null);

        // Act: Dinamik sıralama uygulanıyor.
        var query = _context.TestEntities.AsQueryable().ToDynamic(dynamicQuery);
        var result = query.ToList();

        // Assert: Azalan alfabetik sıralama: Charlie, Bob, Alice
        Assert.Equal(3, result.Count);
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Alice", result[2].Name);
    }

    [Fact]
    public void DynamicFilterAndSort_ShouldReturnFilteredAndOrderedResults()
    {
        // Arrange: "Name contains 'a'" filtresi ve "Name" alanına göre artan sıralama.
        var filter = new Filter
        {
            Field = "Name",
            Operator = "contains",
            Value = "a"
        };
        var sorts = new List<Sort>
            {
                new Sort { Field = "Name", Dir = "asc", Priority = 0 }
            };
        var dynamicQuery = new Dynamic(sorts, filter);

        // Act: Dinamik filtre ve sıralama uygulanıyor.
        var query = _context.TestEntities.AsQueryable().ToDynamic(dynamicQuery);
        var result = query.ToList();

        // Assert: "Charlie" 'a' içeriyor (case-sensitive). Artan sıralama:  "Charlie".
        Assert.Equal(1, result.Count);
        Assert.Equal("Charlie", result[0].Name);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}