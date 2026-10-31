using AwesomeAssertions;
using Kck.Persistence.Abstractions.Specifications;
using Kck.Persistence.EntityFramework.Repositories;
using Kck.Persistence.EntityFramework.Specifications;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Repositories;

public class SpecificationTests
{
    private sealed class ParentsNamed : Specification<CascadeParent>
    {
        public ParentsNamed(string prefix, bool withChildren = false, bool withDeleted = false)
        {
            Where(p => p.Name.StartsWith(prefix));
            ApplyOrderByDescending(p => p.Name);
            if (withChildren)
                AddInclude(p => p.Children);
            if (withDeleted)
                IncludeDeleted();
        }
    }

    private sealed class SecondPageByName : Specification<CascadeParent>
    {
        public SecondPageByName()
        {
            ApplyOrderBy(p => p.Name);
            ApplyPaging(skip: 2, take: 2);
        }
    }

    private sealed class ReferenceByName : Specification<CascadeParent>
    {
        public ReferenceByName()
        {
            Where(p => p.Name == "p1");
            AddInclude(nameof(CascadeParent.Reference));
            AsTracking();
        }
    }

    private static async Task<SqliteHarness<CascadeDbContext>> SeedAsync()
    {
        var harness = new SqliteHarness<CascadeDbContext>(o => new CascadeDbContext(o));
        var reference = new CascadeSingle { Id = Guid.NewGuid(), Name = "r" };
        harness.Context.Parents.AddRange(
            new CascadeParent { Id = Guid.NewGuid(), Name = "p1", Reference = reference, Children = [new() { Id = Guid.NewGuid(), Name = "c" }] },
            new CascadeParent { Id = Guid.NewGuid(), Name = "p2" },
            new CascadeParent { Id = Guid.NewGuid(), Name = "p3", IsDeleted = true, DeletedDate = DateTime.UtcNow },
            new CascadeParent { Id = Guid.NewGuid(), Name = "q4" },
            new CascadeParent { Id = Guid.NewGuid(), Name = "q5" });
        await harness.Context.SaveChangesAsync();
        harness.Context.ChangeTracker.Clear();
        return harness;
    }

    [Fact]
    public async Task ListAsync_CriteriaAndOrder_ExcludesSoftDeletedRows()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<CascadeParent, Guid>(harness.Context);

        var parents = await repo.ListAsync(new ParentsNamed("p"));

        parents.Select(p => p.Name).Should().Equal("p2", "p1");
        harness.Context.ChangeTracker.Entries().Should().BeEmpty(); // no tracking unless the specification asks
    }

    [Fact]
    public async Task ListAsync_IncludeDeleted_ReturnsSoftDeletedRows()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<CascadeParent, Guid>(harness.Context);

        var parents = await repo.ListAsync(new ParentsNamed("p", withDeleted: true));

        parents.Select(p => p.Name).Should().Equal("p3", "p2", "p1");
    }

    [Fact]
    public async Task ListAsync_Include_LoadsTheNavigation()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<CascadeParent, Guid>(harness.Context);

        var parents = await repo.ListAsync(new ParentsNamed("p1", withChildren: true));

        parents.Should().ContainSingle().Which.Children.Should().ContainSingle(c => c.Name == "c");
    }

    [Fact]
    public async Task ListAsync_Paging_SkipsAndTakesInOrder()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<CascadeParent, Guid>(harness.Context);

        var parents = await repo.ListAsync(new SecondPageByName());

        parents.Select(p => p.Name).Should().Equal("q4", "q5");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_StringIncludeAndTracking_LoadsAndTracks()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<CascadeParent, Guid>(harness.Context);

        var parent = await repo.FirstOrDefaultAsync(new ReferenceByName());

        parent!.Reference!.Name.Should().Be("r");
        harness.Context.Entry(parent).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task CountAsync_IgnoresPaging()
    {
        using var harness = await SeedAsync();
        var repo = new EfRepository<CascadeParent, Guid>(harness.Context);

        (await repo.CountAsync(new SecondPageByName())).Should().Be(4);
        (await repo.CountAsync(new ParentsNamed("q"))).Should().Be(2);
    }
}
