using FluentAssertions;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests;

public class EfRepositoryTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly EfRepository<TestEntity, Guid> _sut;

    public EfRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        _sut = new EfRepository<TestEntity, Guid>(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldTrackEntity()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        var result = await _sut.AddAsync(entity);
        await _context.SaveChangesAsync();

        result.Should().BeSameAs(entity);
        _context.TestEntities.Should().ContainSingle(e => e.Name == "Test");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ShouldReturn()
    {
        var id = Guid.NewGuid();
        _context.TestEntities.Add(new TestEntity { Id = id, Name = "Found" });
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Found");
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ShouldReturnNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_SoftDelete_ShouldMarkDeleted()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDelete" };
        _context.TestEntities.Add(entity);
        await _context.SaveChangesAsync();

        await _sut.DeleteAsync(entity, permanent: false);
        await _context.SaveChangesAsync();

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Permanent_ShouldRemoveEntity()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToRemove" };
        _context.TestEntities.Add(entity);
        await _context.SaveChangesAsync();

        await _sut.DeleteAsync(entity, permanent: true);
        await _context.SaveChangesAsync();

        _context.TestEntities.Should().BeEmpty();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        _context.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "A" },
            new TestEntity { Id = Guid.NewGuid(), Name = "B" },
            new TestEntity { Id = Guid.NewGuid(), Name = "C" }
        );
        await _context.SaveChangesAsync();

        var count = await _sut.CountAsync();

        count.Should().Be(3);
    }

    [Fact]
    public async Task AnyAsync_WithMatch_ShouldReturnTrue()
    {
        _context.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "Exists" });
        await _context.SaveChangesAsync();

        var result = await _sut.AnyAsync(e => e.Name == "Exists");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Query_WithSoftDeleted_ShouldExcludeByDefault()
    {
        _context.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "Active" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Deleted", IsDeleted = true, DeletedDate = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var results = _sut.Query().ToList();

        results.Should().ContainSingle(e => e.Name == "Active");
        results.Should().NotContain(e => e.Name == "Deleted");
    }

    [Fact]
    public async Task Query_WithDeletedIncluded_ShouldReturnAll()
    {
        _context.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "Active" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Deleted", IsDeleted = true, DeletedDate = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var results = _sut.Query(withDeleted: true).ToList();

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task RevertSoftDeleteAsync_ShouldClearDeletedFields()
    {
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Revive",
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            DeletedBy = "admin"
        };
        _context.TestEntities.Add(entity);
        await _context.SaveChangesAsync();

        await _sut.RevertSoftDeleteAsync(entity);
        await _context.SaveChangesAsync();

        entity.IsDeleted.Should().BeFalse();
        entity.DeletedDate.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task AddRangeAsync_ShouldTrackAllEntities()
    {
        var entities = new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "R1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "R2" }
        };

        await _sut.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        _context.TestEntities.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAsync_WithPredicate_ShouldReturnMatchingEntity()
    {
        var id = Guid.NewGuid();
        _context.TestEntities.Add(new TestEntity { Id = id, Name = "Match", Value = 42 });
        await _context.SaveChangesAsync();

        var result = await _sut.GetAsync(e => e.Value == 42);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Match");
    }

    [Fact]
    public async Task GetListByDynamicAsync_WithoutWhitelist_ShouldThrow()
    {
        // Arrange — _sut whitelist=null ile olusturuldu (constructor'da verilmedi)
        var dynamicQuery = new DynamicQuery
        {
            Filter = new Filter { Field = "Name", Operator = "eq", Value = "test" },
            Sort = []
        };

        // Act
        var act = () => _sut.GetListByDynamicAsync(dynamicQuery);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*whitelist*");
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
