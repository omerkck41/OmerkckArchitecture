using AwesomeAssertions;
using Kck.Persistence.EntityFramework.Paging;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Repositories;

public class HelperAndFilterTests
{
    // --- SoftDeleteHelper via EfRepository soft-delete cascade ---

    [Fact]
    public async Task SoftDelete_CascadesToCollectionAndReferenceNavigations()
    {
        using var harness = new SqliteHarness<CascadeDbContext>(o => new CascadeDbContext(o));
        var parent = new CascadeParent
        {
            Id = Guid.NewGuid(),
            Name = "P",
            Children =
            [
                new CascadeChild { Id = Guid.NewGuid(), Name = "C1" },
                new CascadeChild { Id = Guid.NewGuid(), Name = "C2" },
            ],
            Reference = new CascadeSingle { Id = Guid.NewGuid(), Name = "S" },
        };
        harness.Context.Parents.Add(parent);
        await harness.Context.SaveChangesAsync();

        // Reload with navigations populated so CascadeSoftDelete sees CurrentValue.
        var loaded = await harness.Context.Parents
            .Include(p => p.Children)
            .Include(p => p.Reference)
            .SingleAsync(p => p.Id == parent.Id);

        var repo = new EfRepository<CascadeParent, Guid>(harness.Context);
        await repo.DeleteAsync(loaded, permanent: false);
        await harness.Context.SaveChangesAsync();

        var children = await harness.Context.Children.IgnoreQueryFilters().ToListAsync();
        var single = await harness.Context.Singles.IgnoreQueryFilters().SingleAsync();

        loaded.IsDeleted.Should().BeTrue();
        children.Should().OnlyContain(c => c.IsDeleted);
        single.IsDeleted.Should().BeTrue();
    }

    // Note: BulkOperationHelper.BulkUpdateAsync (ExecuteUpdate with a boxed object value)
    // is exercised by the PostgreSQL Testcontainers integration suite — SQLite cannot assign
    // a type mapping to the untyped '@value' parameter, so it is intentionally not covered here.

    // --- ModelBuilderExtensions: global soft-delete query filter ---

    [Fact]
    public async Task GlobalSoftDeleteFilter_ExcludesDeletedRows()
    {
        using var harness = new SqliteHarness<GlobalFilterDbContext>(o => new GlobalFilterDbContext(o));
        harness.Context.Items.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "Active" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Gone", IsDeleted = true, DeletedDate = DateTime.UtcNow });
        await harness.Context.SaveChangesAsync();

        var visible = await harness.Context.Items.ToListAsync();
        var all = await harness.Context.Items.IgnoreQueryFilters().ToListAsync();

        visible.Should().ContainSingle(e => e.Name == "Active");
        all.Should().HaveCount(2);
    }

    // --- QueryablePaginateExtensions: from/index guard ---

    [Fact]
    public async Task ToPaginateAsync_FromGreaterThanIndex_Throws()
    {
        using var harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));

        var act = () => harness.Context.TestEntities.ToPaginateAsync(index: 0, size: 10, from: 1);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*From*");
    }

    [Fact]
    public async Task ToPaginateAsync_FromLessThanIndex_ReturnsPage()
    {
        // from < index is valid (no throw). A '<' mutant of the guard would throw here instead,
        // and because this path does not raise, Stryker reliably credits the coverage.
        using var harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        harness.Context.TestEntities.AddRange(
            Enumerable.Range(0, 5).Select(i => new TestEntity { Id = Guid.NewGuid(), Name = $"N{i}" }));
        await harness.Context.SaveChangesAsync();

        var page = await harness.Context.TestEntities.OrderBy(e => e.Name)
            .ToPaginateAsync(index: 1, size: 2, from: 0);

        page.Count.Should().Be(5);
        page.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ToPaginateAsync_FromEqualsIndex_ReturnsPage()
    {
        using var harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        harness.Context.TestEntities.AddRange(
            Enumerable.Range(0, 5).Select(i => new TestEntity { Id = Guid.NewGuid(), Name = $"N{i}" }));
        await harness.Context.SaveChangesAsync();

        var page = await harness.Context.TestEntities.OrderBy(e => e.Name)
            .ToPaginateAsync(index: 1, size: 2, from: 1);

        page.Count.Should().Be(5);
        page.Items.Should().HaveCount(2);
    }
}
