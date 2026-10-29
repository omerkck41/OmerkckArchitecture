using AwesomeAssertions;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Repositories;

/// <summary>
/// v4 (ADR-0028 K3): the QueryOptions-based read methods are IReadRepository members; the bool-flag overloads (KCK0100)
/// are gone. Calls without options keep the 3.x defaults (Get/GetList/GetListByDynamic track, Any/GetById/Count do not).
/// </summary>
public sealed class QueryOptionsRepositoryTests : IDisposable
{
    private readonly SqliteHarness<TestDbContext> _harness;
    private readonly TestDbContext _ctx;
#pragma warning disable CA1859 // the tests exercise the IReadRepository contract, not the concrete type
    private readonly IReadRepository<TestEntity, Guid> _sut;
#pragma warning restore CA1859

    public QueryOptionsRepositoryTests()
    {
        _harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        _ctx = _harness.Context;
        _sut = new EfRepository<TestEntity, Guid>(_ctx);
    }

    private async Task<TestEntity> Seed(string name, bool deleted = false)
    {
        var e = new TestEntity { Id = Guid.NewGuid(), Name = name, Value = 1, IsDeleted = deleted };
        if (deleted)
            e.DeletedDate = DateTime.UtcNow;
        _ctx.TestEntities.Add(e);
        await _ctx.SaveChangesAsync();
        _ctx.Entry(e).State = EntityState.Detached;
        return e;
    }

    [Fact]
    public async Task Query_ObsoleteBoolOverload_StillReturnsDeletedAndTrackedRows()
    {
        await Seed("Deleted", deleted: true);
        var repository = new EfRepository<TestEntity, Guid>(_ctx);

#pragma warning disable KCK0101 // the deprecated overload keeps working until v5.0
        var item = repository.Query(withDeleted: true, enableTracking: true).Single();
#pragma warning restore KCK0101

        item.IsDeleted.Should().BeTrue();
        _ctx.Entry(item).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public void IQuery_QueryOptionsOverload_DefaultsToTheBoolOverloadOfOtherImplementations()
    {
        var rows = new[] { new TestEntity { Name = "Active" }, new TestEntity { Name = "Deleted", IsDeleted = true } };
        IQuery<TestEntity> query = new ListQuery(rows);

        query.Query(QueryOptions.WithDeleted, e => e.Name.Length > 0).Should().HaveCount(2);
        query.Query(QueryOptions.None).Should().ContainSingle().Which.Name.Should().Be("Active");
    }

    // An IQuery implemented before the QueryOptions member existed.
    private sealed class ListQuery(IEnumerable<TestEntity> rows) : IQuery<TestEntity>
    {
        [Obsolete("Implements the deprecated member.", DiagnosticId = "KCK0101")]
        public IQueryable<TestEntity> Query(
            System.Linq.Expressions.Expression<Func<TestEntity, bool>>? filter = null,
            Func<IQueryable<TestEntity>, IOrderedQueryable<TestEntity>>? orderBy = null,
            bool withDeleted = false,
            bool enableTracking = false)
        {
            var query = rows.AsQueryable().Where(e => withDeleted || !e.IsDeleted);
            return filter is null ? query : query.Where(filter);
        }
    }

    [Fact]
    public void IReadRepository_HasNoBoolFlagOverloads()
    {
        var boolParameters = typeof(IReadRepository<,>).GetMethods()
            .SelectMany(m => m.GetParameters().Select(p => (Method: m.Name, p.Name, p.ParameterType)))
            .Where(p => p.ParameterType == typeof(bool))
            .Select(p => $"{p.Method}({p.Name})");

        boolParameters.Should().BeEmpty();
        typeof(IReadRepository<,>).Assembly.GetType("Kck.Persistence.Abstractions.Repositories.ReadRepositoryExtensions")
            .Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithQueryOptions_HonoursIncludeDeleted()
    {
        await Seed("gone", deleted: true);

        (await _sut.GetAsync(e => e.Name == "gone")).Should().BeNull();
        (await _sut.GetAsync(e => e.Name == "gone", QueryOptions.WithDeleted)).Should().NotBeNull();
    }

    [Fact]
    public async Task ReadsWithoutOptions_KeepTheirPreviousTrackingDefaults()
    {
        var seeded = await Seed("item");

        var viaGet = await _sut.GetAsync(e => e.Id == seeded.Id);
        _ctx.Entry(viaGet!).State.Should().Be(EntityState.Unchanged);
        _ctx.Entry(viaGet!).State = EntityState.Detached;

        var viaList = (await _sut.GetListAsync(size: 10)).Items.Single();
        _ctx.Entry(viaList).State.Should().Be(EntityState.Unchanged);
        _ctx.Entry(viaList).State = EntityState.Detached;

        var viaId = await _sut.GetByIdAsync(seeded.Id);
        _ctx.Entry(viaId!).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task ExplicitOptions_OverrideTheDefaults()
    {
        var seeded = await Seed("item");

        var untracked = await _sut.GetAsync(e => e.Id == seeded.Id, QueryOptions.None);
        _ctx.Entry(untracked!).State.Should().Be(EntityState.Detached);

        var tracked = await _sut.GetByIdAsync(seeded.Id, QueryOptions.Tracking);
        _ctx.Entry(tracked!).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task AnyCountAndList_AcceptOptionsAndPredicate()
    {
        await Seed("live");
        await Seed("gone", deleted: true);

        (await _sut.CountAsync()).Should().Be(1);
        (await _sut.CountAsync(QueryOptions.WithDeleted)).Should().Be(2);
        (await _sut.AnyAsync(QueryOptions.None, e => e.Name == "gone")).Should().BeFalse();
        (await _sut.AnyAsync(QueryOptions.WithDeleted, e => e.Name == "gone")).Should().BeTrue();
        (await _sut.GetListAsync(QueryOptions.WithDeleted, e => e.Name == "gone")).Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetListByDynamicAsync_WithoutOptions_TracksAndExcludesSoftDeleted()
    {
        await Seed("live");
        await Seed("live", deleted: true);
        var repo = new EfRepository<TestEntity, Guid>(_ctx, new TestWhitelist<TestEntity>("Name"));
        var query = new DynamicQuery { Filter = new Filter { Field = "Name", Operator = "eq", Value = "live" } };

        var item = (await repo.GetListByDynamicAsync(query)).Items.Single();

        _ctx.Entry(item).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetListAndGetById_WithoutOptions_ExcludeSoftDeletedRows()
    {
        var deleted = await Seed("gone", deleted: true);

        (await _sut.GetListAsync()).Items.Should().BeEmpty();
        (await _sut.GetByIdAsync(deleted.Id)).Should().BeNull();
        (await _sut.GetByIdAsync(deleted.Id, QueryOptions.WithDeleted)).Should().NotBeNull();
    }

    public void Dispose() => _harness.Dispose();
}
