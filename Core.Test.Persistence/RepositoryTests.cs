using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.Persistence.Repositories;
using Core.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace TestPersistence;

// Repository, UnitOfWork ve CRUD işlemlerini test eden sınıf
public class RepositoryTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly IAsyncRepository<TestEntity, int> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RepositoryTests()
    {
        // Her test için benzersiz bir InMemory veritabanı oluşturulur.
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);

        // UnitOfWork ve Repository oluşturuluyor.
        _unitOfWork = new UnitOfWork<TestDbContext>(_context);
        _repository = _unitOfWork.Repository<TestEntity, int>();
    }

    [Fact]
    public async Task AddEntity_ShouldSetCreatedProperties()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test1" };

        // Act
        var addedEntity = await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.NotEqual(default(DateTime), addedEntity.CreatedDate);
        Assert.False(addedEntity.IsDeleted);
        Assert.Equal("ArchonApp-System", addedEntity.CreatedBy);
    }

    [Fact]
    public async Task UpdateEntity_ShouldSetModifiedProperties()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test2" };
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Act
        entity.Name = "Test2Updated";
        var updatedEntity = await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.NotNull(updatedEntity.ModifiedDate);
        Assert.Equal("ArchonApp-System", updatedEntity.ModifiedBy);
        Assert.Equal("Test2Updated", updatedEntity.Name);
    }

    [Fact]
    public async Task SoftDeleteEntity_ShouldMarkAsDeletedAndFilterOut()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test3" };
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Act: Soft delete (permanent false)
        var deletedEntity = await _repository.DeleteAsync(entity, permanent: false);
        await _unitOfWork.SaveChangesAsync();

        // Assert: Audit bilgilerinin ayarlandığını doğrula
        Assert.True(deletedEntity.IsDeleted);
        Assert.NotNull(deletedEntity.DeletedDate);
        Assert.Equal("System", deletedEntity.DeletedBy);

        // Global query filter nedeniyle, soft deleted entity normal sorgularda gelmemeli
        var list = await _repository.GetListAsync();
        Assert.DoesNotContain(list.Items, e => e.Id == entity.Id);

        // IgnoreQueryFilters kullanılarak soft deleted entity'ye erişilebildiğini doğrula
        var softDeleted = await _context.TestEntities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == entity.Id);
        Assert.NotNull(softDeleted);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task HardDeleteEntity_ShouldRemoveEntityFromDatabase()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test4" };
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Act: Hard delete (permanent true)
        await _repository.DeleteAsync(entity, permanent: true);
        await _unitOfWork.SaveChangesAsync();

        // Assert: Entity artık veritabanında bulunmamalı. GetById çağrısında exception fırlatılmalı.
        await Assert.ThrowsAsync<CustomException>(async () => await _repository.GetByIdAsync(entity.Id));
    }

    [Fact]
    public async Task RevertSoftDelete_ShouldRestoreEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test5" };
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Act: Önce soft delete yap, sonra revert et.
        await _repository.DeleteAsync(entity, permanent: false);
        await _unitOfWork.SaveChangesAsync();

        var restored = await _repository.RevertSoftDeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Assert: Entity tekrar aktif hale gelmeli.
        Assert.False(restored.IsDeleted);
        Assert.Null(restored.DeletedDate);
        Assert.Null(restored.DeletedBy);

        var list = await _repository.GetListAsync();
        Assert.Contains(list.Items, e => e.Id == restored.Id);
    }

    [Fact]
    public async Task BulkUpdate_ShouldUpdateMultipleEntities()
    {
        // Arrange
        var entity1 = new TestEntity { Name = "Bulk1" };
        var entity2 = new TestEntity { Name = "Bulk2" };
        await _repository.AddAsync(entity1);
        await _repository.AddAsync(entity2);
        await _unitOfWork.SaveChangesAsync();

        // Act: Bulk update ile tüm entity'lerin Name alanını güncelle
        await _repository.BulkUpdateAsync(e => e.Name.StartsWith("Bulk"), updates: (e => e.Name, "UpdatedBulk"));
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var list = await _repository.GetListAsync(e => e.Name == "UpdatedBulk");
        Assert.Equal(2, list.Items.Count);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}