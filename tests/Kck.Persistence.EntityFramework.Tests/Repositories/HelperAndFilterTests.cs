using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Paging;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Repositories;

public class HelperAndFilterTests
{
    // --- SoftDeleteHelper via EfRepository soft-delete cascade ---

    [Fact]
    public async Task SoftDelete_CascadesToDependentsButNotToTheReferencedPrincipal()
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
        // Reference is the principal the parent points at, not a dependent of it.
        single.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDelete_DoesNotDeleteTheOtherSideOfAManyToMany()
    {
        using var harness = new SqliteHarness<ArticleDbContext>(o => new ArticleDbContext(o));
        var article = new Article { Id = Guid.NewGuid(), Labels = [new Label { Id = Guid.NewGuid() }] };
        harness.Context.Articles.Add(article);
        await harness.Context.SaveChangesAsync();
        harness.Context.ChangeTracker.Clear();
        var loaded = await harness.Context.Articles.Include(a => a.Labels).SingleAsync();

        await new EfRepository<Article, Guid>(harness.Context).DeleteAsync(loaded, permanent: false);
        await harness.Context.SaveChangesAsync();

        (await harness.Context.Labels.IgnoreQueryFilters().AsNoTracking().SingleAsync()).IsDeleted.Should().BeFalse();
    }

    public sealed class Article : FullEntity<Guid>
    {
        public List<Label> Labels { get; set; } = [];
    }

    public sealed class Label : FullEntity<Guid>;

    public sealed class Profile : FullEntity<Guid>
    {
        public ProfileDetail? Detail { get; set; }
    }

    public sealed class ProfileDetail : FullEntity<Guid>
    {
        public Guid ProfileId { get; set; }
        public Profile? Profile { get; set; }
    }

    public sealed class ArticleDbContext(DbContextOptions<ArticleDbContext> options) : DbContext(options)
    {
        public DbSet<Article> Articles => Set<Article>();
        public DbSet<Label> Labels => Set<Label>();
        public DbSet<Profile> Profiles => Set<Profile>();
        public DbSet<ProfileDetail> ProfileDetails => Set<ProfileDetail>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>(b =>
            {
                b.Ignore(e => e.RowVersion);
                b.HasMany(e => e.Labels).WithMany();
            });
            modelBuilder.Entity<Label>(b => b.Ignore(e => e.RowVersion));
            modelBuilder.Entity<Profile>(b =>
            {
                b.Ignore(e => e.RowVersion);
                b.HasOne(p => p.Detail).WithOne(d => d.Profile).HasForeignKey<ProfileDetail>(d => d.ProfileId);
            });
            modelBuilder.Entity<ProfileDetail>(b => b.Ignore(e => e.RowVersion));
        }
    }

    [Fact]
    public async Task SoftDelete_DoesNotDeleteASharedPrincipal()
    {
        using var harness = new SqliteHarness<CascadeDbContext>(o => new CascadeDbContext(o));
        var shared = new CascadeSingle { Id = Guid.NewGuid(), Name = "shared" };
        var deleted = new CascadeParent { Id = Guid.NewGuid(), Name = "deleted", Reference = shared };
        var other = new CascadeParent { Id = Guid.NewGuid(), Name = "other", Reference = shared };
        harness.Context.Parents.AddRange(deleted, other);
        await harness.Context.SaveChangesAsync();
        harness.Context.ChangeTracker.Clear();
        var loaded = await harness.Context.Parents.Include(p => p.Reference).SingleAsync(p => p.Id == deleted.Id);

        await new EfRepository<CascadeParent, Guid>(harness.Context).DeleteAsync(loaded, permanent: false);
        await harness.Context.SaveChangesAsync();

        (await harness.Context.Singles.IgnoreQueryFilters().AsNoTracking().SingleAsync()).IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDelete_OneToOnePrincipal_SoftDeletesItsDependent()
    {
        using var harness = new SqliteHarness<ArticleDbContext>(o => new ArticleDbContext(o));
        harness.Context.Profiles.Add(new Profile { Id = Guid.NewGuid(), Detail = new ProfileDetail { Id = Guid.NewGuid() } });
        await harness.Context.SaveChangesAsync();
        harness.Context.ChangeTracker.Clear();
        var loaded = await harness.Context.Profiles.Include(p => p.Detail).SingleAsync();

        await new EfRepository<Profile, Guid>(harness.Context).DeleteAsync(loaded, permanent: false);
        await harness.Context.SaveChangesAsync();

        (await harness.Context.ProfileDetails.IgnoreQueryFilters().AsNoTracking().SingleAsync()).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDelete_OneToOneDependent_LeavesItsPrincipal()
    {
        using var harness = new SqliteHarness<ArticleDbContext>(o => new ArticleDbContext(o));
        harness.Context.Profiles.Add(new Profile { Id = Guid.NewGuid(), Detail = new ProfileDetail { Id = Guid.NewGuid() } });
        await harness.Context.SaveChangesAsync();
        harness.Context.ChangeTracker.Clear();
        var detail = await harness.Context.ProfileDetails.Include(d => d.Profile).SingleAsync();

        await new EfRepository<ProfileDetail, Guid>(harness.Context).DeleteAsync(detail, permanent: false);
        await harness.Context.SaveChangesAsync();

        (await harness.Context.Profiles.IgnoreQueryFilters().AsNoTracking().SingleAsync()).IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDelete_OneToOnePrincipalWithoutDependent_DeletesOnlyItself()
    {
        using var harness = new SqliteHarness<ArticleDbContext>(o => new ArticleDbContext(o));
        harness.Context.Profiles.Add(new Profile { Id = Guid.NewGuid() });
        await harness.Context.SaveChangesAsync();
        harness.Context.ChangeTracker.Clear();
        var loaded = await harness.Context.Profiles.Include(p => p.Detail).SingleAsync();

        await new EfRepository<Profile, Guid>(harness.Context).DeleteAsync(loaded, permanent: false);
        await harness.Context.SaveChangesAsync();

        (await harness.Context.Profiles.IgnoreQueryFilters().AsNoTracking().SingleAsync()).IsDeleted.Should().BeTrue();
    }

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
