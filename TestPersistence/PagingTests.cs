using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore;

namespace TestPersistence;

public class PagingTests : IDisposable
{
    private readonly TestDbContext _context;

    public PagingTests()
    {
        // Her test için benzersiz bir InMemory veritabanı oluşturuyoruz.
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);

        // Test verisi: 10 adet TestEntity oluşturuyoruz.
        for (int i = 1; i <= 10; i++)
        {
            _context.TestEntities.Add(new TestEntity { Id = i, Name = $"Test{i}" });
        }
        _context.SaveChanges();
    }

    [Fact]
    public async Task ToPaginateAsync_ReturnsCorrectPageMetadata()
    {
        // Arrange
        // Kayıtları Id'ye göre artan sırada düzenleyelim.
        var query = _context.TestEntities.OrderBy(e => e.Id).AsQueryable();
        int pageIndex = 2; // 2. sayfa
        int pageSize = 3;  // Sayfa başına 3 kayıt

        // Act
        IPaginate<TestEntity> paginate = await query.ToPaginateAsync(pageIndex, pageSize, from: 0);

        // Assert
        Assert.Equal(pageIndex, paginate.Index);
        Assert.Equal(pageSize, paginate.Size);
        Assert.Equal(10, paginate.TotalRecords);
        // Count, paginate oluşturulurken kullanılan toplam kayıt sayısı (10)
        Assert.Equal(10, paginate.Count);
        int expectedPages = (int)Math.Ceiling(10 / (double)pageSize);
        Assert.Equal(expectedPages, paginate.Pages);

        // 2. sayfada 3 kayıt olmalı: 4., 5., 6. kayıtlar
        Assert.Equal(3, paginate.Items.Count);
        Assert.Equal("Test7", paginate.Items[0].Name);

        // Sayfa 2 olduğundan, önceki sayfa var (HasPrevious true) ve sonraki sayfa var (HasNext true)
        Assert.True(paginate.HasPrevious);
        Assert.True(paginate.HasNext);
        Assert.False(paginate.IsFirstPage);
        Assert.False(paginate.IsLastPage);
    }

    [Fact]
    public void ToPaginate_Synchronous_ReturnsCorrectPageMetadata()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id).AsQueryable();
        int pageIndex = 3; // 3. sayfa. sayfalama 0 dan başlar
        int pageSize = 3;  // Sayfa başına 3 kayıt
                           // 10 kayıttan, 4. sayfada sadece 1 kayıt (10 - 3*3 = 1) bulunacak

        // Act
        IPaginate<TestEntity> paginate = query.ToPaginate(pageIndex, pageSize, from: 0);

        // Assert
        Assert.Equal(pageIndex, paginate.Index);
        Assert.Equal(pageSize, paginate.Size);
        Assert.Equal(10, paginate.TotalRecords);
        int expectedPages = (int)Math.Ceiling(10 / (double)pageSize);
        Assert.Equal(expectedPages, paginate.Pages);

        // 4. sayfada tek kayıt olmalı
        Assert.Single(paginate.Items);
        Assert.Equal("Test10", paginate.Items.First().Name);

        // 4. sayfa, toplam 4 sayfadan oluşuyorsa:
        Assert.True(paginate.HasPrevious);
        Assert.False(paginate.HasNext);
        Assert.False(paginate.IsFirstPage);
        Assert.True(paginate.IsLastPage);
    }

    [Fact]
    public async Task ToPaginateAsync_ThrowsException_WhenFromGreaterThanIndex()
    {
        // Arrange
        var query = _context.TestEntities.AsQueryable();
        // from değeri, index değerinden büyük olmamalıdır. Örneğin: from = 3, index = 2 olursa exception fırlatmalı.

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await query.ToPaginateAsync(2, 3, from: 3)
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}