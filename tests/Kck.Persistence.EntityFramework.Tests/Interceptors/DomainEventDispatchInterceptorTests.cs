using AwesomeAssertions;
using Kck.Persistence.EntityFramework.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Interceptors;

public class DomainEventDispatchInterceptorTests
{
    private static TestDbContext NewContext(
        CapturingDispatcher dispatcher,
        ILogger<DomainEventDispatchInterceptor> logger)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new DomainEventDispatchInterceptor(dispatcher, logger))
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task SavedChangesAsync_DispatchesPendingDomainEvents()
    {
        var dispatcher = new CapturingDispatcher();
        var logger = Substitute.For<ILogger<DomainEventDispatchInterceptor>>();
        using var ctx = NewContext(dispatcher, logger);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        entity.AddDomainEvent(new TestDomainEvent());
        ctx.TestEntities.Add(entity);

        await ctx.SaveChangesAsync();

        dispatcher.CallCount.Should().Be(1);
        dispatcher.Received.Should().ContainSingle();
    }

    // An application may hide the base members with `new`; the interceptor must still use Entity<TId>'s own.
    private sealed class HidingEntity : Kck.Core.Abstractions.Entities.Entity<Guid>
    {
        public new IReadOnlyList<Kck.Core.Abstractions.Entities.IDomainEvent> DomainEvents => [.. base.DomainEvents];

        public int HiddenClearCalls { get; private set; }

        // Deliberately does not clear: the interceptor must clear the base list.
        public new void ClearDomainEvents() => HiddenClearCalls++;
    }

    private sealed class HidingContext(DbContextOptions<HidingContext> options) : DbContext(options)
    {
        public DbSet<HidingEntity> Items => Set<HidingEntity>();
    }

    [Fact]
    public async Task SavedChangesAsync_EntityHidesDomainEventMembers_DispatchesAndClearsTheBaseList()
    {
        var dispatcher = new CapturingDispatcher();
        var options = new DbContextOptionsBuilder<HidingContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new DomainEventDispatchInterceptor(dispatcher, Substitute.For<ILogger<DomainEventDispatchInterceptor>>()))
            .Options;
        await using var ctx = new HidingContext(options);
        var entity = new HidingEntity { Id = Guid.NewGuid() };
        entity.AddDomainEvent(new TestDomainEvent());
        ctx.Items.Add(entity);

        await ctx.SaveChangesAsync();

        dispatcher.Received.Should().ContainSingle();
        Kck.Core.Abstractions.Entities.Entity<Guid> asBase = entity; // the base list, not the hiding property
        asBase.DomainEvents.Should().BeEmpty();
        entity.HiddenClearCalls.Should().Be(0);
    }

    [Fact]
    public async Task SavedChangesAsync_NoDomainEvents_DoesNotDispatch()
    {
        var dispatcher = new CapturingDispatcher();
        var logger = Substitute.For<ILogger<DomainEventDispatchInterceptor>>();
        using var ctx = NewContext(dispatcher, logger);

        ctx.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "A" });

        await ctx.SaveChangesAsync();

        dispatcher.CallCount.Should().Be(0);
    }

    [Fact]
    public void SavedChanges_Sync_DispatchesAndWarns()
    {
        var dispatcher = new CapturingDispatcher();
        var logger = Substitute.For<ILogger<DomainEventDispatchInterceptor>>();
        logger.IsEnabled(LogLevel.Warning).Returns(true);
        using var ctx = NewContext(dispatcher, logger);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        entity.AddDomainEvent(new TestDomainEvent());
        ctx.TestEntities.Add(entity);

        ctx.SaveChanges();

        dispatcher.CallCount.Should().Be(1);
        dispatcher.Received.Should().ContainSingle();
        logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
