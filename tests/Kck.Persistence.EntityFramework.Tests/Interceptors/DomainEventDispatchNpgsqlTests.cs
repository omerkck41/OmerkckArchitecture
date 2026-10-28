using AwesomeAssertions;
using Kck.Persistence.EntityFramework.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Interceptors;

/// <summary>
/// Npgsql may reuse the DbTransaction object on a connector; events of a transaction disposed uncommitted must not
/// ride along with the next one (including EF's own SaveChanges transaction).
/// </summary>
/// <remarks>Requires Docker. CI: ubuntu-only (Category=Integration filter).</remarks>
[Trait("Category", "Integration")]
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class DomainEventDispatchNpgsqlTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("kck_test")
        .WithUsername("kck")
        .WithPassword("kck-test-password")
        .Build();

    private readonly CapturingDispatcher _dispatcher = new();
    private TestDbContext _ctx = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .AddInterceptors(new DomainEventDispatchInterceptor(
                _dispatcher, NullLogger<DomainEventDispatchInterceptor>.Instance))
            .Options;
        _ctx = new TestDbContext(options);
        await _ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private void AddWithEvent(out TestDomainEvent domainEvent)
    {
        domainEvent = new TestDomainEvent();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        entity.AddDomainEvent(domainEvent);
        _ctx.TestEntities.Add(entity);
    }

    [Fact]
    public async Task TransactionDisposedWithoutCommit_EventsAreNotDispatchedByTheNextSave()
    {
        await using (await _ctx.Database.BeginTransactionAsync())
        {
            AddWithEvent(out _);
            await _ctx.SaveChangesAsync();
        }

        _ctx.ChangeTracker.Clear();
        AddWithEvent(out var kept);
        await _ctx.SaveChangesAsync();

        _dispatcher.Received.Should().Equal(kept);
    }
}
