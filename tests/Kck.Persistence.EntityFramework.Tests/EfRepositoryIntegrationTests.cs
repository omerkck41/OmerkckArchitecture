using FluentAssertions;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests;

/// <summary>
/// LS-FAZ-4 PoC: gercek PostgreSQL container ile EF Core integration testleri.
/// InMemory provider'in kacirdigi davranislari (concurrency token, transaction
/// boundary, JSON ignored mappings vb.) burada yakalar.
/// </summary>
/// <remarks>
/// Kosma kosullari:
/// - Yerel: Docker Desktop calisiyor olmali.
/// - CI: ubuntu-latest runner'da Docker pre-installed; windows-latest skip eder
///   (ci/build-test.yml integration test step'i sadece ubuntu).
/// xUnit Trait "Category=Integration" ile filtreleniyor.
/// </remarks>
[Trait("Category", "Integration")]
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal — analyzer tanimiyor
public sealed class EfRepositoryIntegrationTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("kck_test")
        .WithUsername("kck")
        .WithPassword("kck-test-password")
        .Build();

    private TestDbContext _context = default!;
    private EfRepository<TestEntity, Guid> _sut = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new TestDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _sut = new EfRepository<TestEntity, Guid>(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_PersistsToRealDatabase()
    {
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Integration",
            Value = 42,
        };

        await _sut.AddAsync(entity);
        await _context.SaveChangesAsync();

        var refetched = await _context.TestEntities.AsNoTracking()
            .SingleAsync(e => e.Id == entity.Id);

        refetched.Name.Should().Be("Integration");
        refetched.Value.Should().Be(42);
        refetched.CreatedDate.Should().NotBe(default);
    }

    [Fact]
    public async Task SoftDelete_FlagsEntity_WithoutPhysicalRemoval()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDelete" };
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        entity.IsDeleted = true;
        entity.DeletedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var stillExists = await _context.TestEntities.AsNoTracking()
            .AnyAsync(e => e.Id == entity.Id);
        var marked = await _context.TestEntities.AsNoTracking()
            .SingleAsync(e => e.Id == entity.Id);

        stillExists.Should().BeTrue("soft delete record'u fiziksel silmemeli");
        marked.IsDeleted.Should().BeTrue();
        marked.DeletedDate.Should().NotBeNull();
    }
}
