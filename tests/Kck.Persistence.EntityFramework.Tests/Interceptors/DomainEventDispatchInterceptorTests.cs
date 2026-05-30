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
