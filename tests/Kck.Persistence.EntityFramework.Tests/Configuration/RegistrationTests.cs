using AwesomeAssertions;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Kck.Persistence.EntityFramework.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Configuration;

public class RegistrationTests
{
    [Fact]
    public void AddKckPersistence_InvokesConfigure_AndReturnsSameCollection()
    {
        var services = new ServiceCollection();
        var configured = false;

        var result = services.AddKckPersistence(_ => configured = true);

        configured.Should().BeTrue("configure callback must be invoked");
        result.Should().BeSameAs(services);
    }

    [Fact]
    public async Task UseEntityFramework_RegistersContextUnitOfWorkAndFactory()
    {
        var services = new ServiceCollection();

        services.AddKckPersistence(b =>
            b.UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase("reg-test")));

        var sp = services.BuildServiceProvider();
        // EfUnitOfWork is IAsyncDisposable-only — the scope must be disposed asynchronously.
        await using var scope = sp.CreateAsyncScope();

        scope.ServiceProvider.GetService<TestDbContext>().Should().NotBeNull();
        scope.ServiceProvider.GetService<DbContext>().Should().BeOfType<TestDbContext>();
        scope.ServiceProvider.GetService<IEfRepositoryFactory>().Should().BeOfType<DefaultEfRepositoryFactory>();
        scope.ServiceProvider.GetService<IUnitOfWork>().Should().BeOfType<EfUnitOfWork>();
    }

    [Fact]
    public void UseEntityFramework_ReturnsSameBuilder_ForChaining()
    {
        var builder = new KckPersistenceBuilder(new ServiceCollection());

        var returned = builder.UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase("chain"));

        returned.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddAuditInterceptor_RegistersInterceptorSingleton()
    {
        var services = new ServiceCollection();

        new KckPersistenceBuilder(services).AddAuditInterceptor();

        var descriptor = services.Single(d => d.ServiceType == typeof(AuditInterceptor));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddDomainEventDispatch_RegistersDispatcherAndInterceptor()
    {
        var services = new ServiceCollection();

        new KckPersistenceBuilder(services).AddDomainEventDispatch<CapturingDispatcher>();

        services.Should().Contain(d =>
            d.ServiceType == typeof(IDomainEventDispatcher)
            && d.ImplementationType == typeof(CapturingDispatcher)
            && d.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(d =>
            d.ServiceType == typeof(DomainEventDispatchInterceptor)
            && d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task AddAuditInterceptor_WiresInterceptorIntoDbContext()
    {
        var services = new ServiceCollection();
        services.AddKckPersistence(b => b
            .UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .AddAuditInterceptor());

        await using var sp = services.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A", CreatedDate = default };
        ctx.TestEntities.Add(entity);

        await ctx.SaveChangesAsync();

        entity.CreatedDate.Should().NotBe(default);
    }

    [Fact]
    public async Task AddDomainEventDispatch_WiresInterceptorIntoDbContext()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddKckPersistence(b => b
            .UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .AddDomainEventDispatch<CapturingDispatcher>());

        await using var sp = services.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        entity.AddDomainEvent(new TestDomainEvent());
        ctx.TestEntities.Add(entity);

        await ctx.SaveChangesAsync();

        var dispatcher = (CapturingDispatcher)scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        dispatcher.Received.Should().ContainSingle();
    }

    [Fact]
    public async Task AddAuditInterceptor_AlreadyAddedManually_IsNotAddedTwice()
    {
        var services = new ServiceCollection();
        services.AddKckPersistence(b => b
            .UseEntityFramework<TestDbContext>(o => o
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(new AuditInterceptor()))
            .AddAuditInterceptor());

        await using var sp = services.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<TestDbContext>>();

        options.FindExtension<CoreOptionsExtension>()!.Interceptors
            .Should().ContainSingle(i => i is AuditInterceptor);
    }

    [Fact]
    public async Task UseEntityFramework_WithoutInterceptorRegistrations_AddsNoInterceptors()
    {
        var services = new ServiceCollection();
        services.AddKckPersistence(b =>
            b.UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString())));

        await using var sp = services.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<TestDbContext>>();

        (options.FindExtension<CoreOptionsExtension>()?.Interceptors ?? []).Should().BeEmpty();
    }

    [Fact]
    public async Task AddAuditInterceptor_PermanentDelete_RemovesRow()
    {
        var services = new ServiceCollection();
        services.AddKckPersistence(b => b
            .UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .AddAuditInterceptor());

        await using var sp = services.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        await uow.Repository<TestEntity, Guid>().AddAsync(entity);
        await uow.SaveChangesAsync();

        await uow.Repository<TestEntity, Guid>().DeleteAsync(entity.Id, permanent: true);
        await uow.SaveChangesAsync();

        (await ctx.TestEntities.IgnoreQueryFilters().AnyAsync(e => e.Id == entity.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task AddAuditInterceptor_PermanentDeleteRange_RemovesRows()
    {
        var services = new ServiceCollection();
        services.AddKckPersistence(b => b
            .UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .AddAuditInterceptor());

        await using var sp = services.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        TestEntity[] entities = [new() { Id = Guid.NewGuid(), Name = "A" }, new() { Id = Guid.NewGuid(), Name = "B" }];
        await uow.Repository<TestEntity, Guid>().AddRangeAsync(entities);
        await uow.SaveChangesAsync();

        await uow.Repository<TestEntity, Guid>().DeleteRangeAsync(entities, permanent: true);
        await uow.SaveChangesAsync();

        (await ctx.TestEntities.IgnoreQueryFilters().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AddAuditInterceptor_AfterPermanentDeleteIsSaved_LaterRemoveOfSameInstanceIsSoft()
    {
        var services = new ServiceCollection();
        services.AddKckPersistence(b => b
            .UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .AddAuditInterceptor());

        await using var sp = services.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();
        await uow.Repository<TestEntity, Guid>().DeleteAsync(entity, permanent: true);
        await ctx.SaveChangesAsync();

        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();
        ctx.TestEntities.Remove(entity);
        await ctx.SaveChangesAsync();

        entity.IsDeleted.Should().BeTrue();
        (await ctx.TestEntities.IgnoreQueryFilters().AnyAsync(e => e.Id == entity.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task AddAuditInterceptor_RegisteredBeforeUseEntityFramework_IsStillWired()
    {
        var services = new ServiceCollection();
        services.AddKckPersistence(b => b
            .AddAuditInterceptor()
            .UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString())));

        await using var sp = services.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A", CreatedDate = default };
        ctx.TestEntities.Add(entity);

        await ctx.SaveChangesAsync();

        entity.CreatedDate.Should().NotBe(default);
    }

    [Fact]
    public void DefaultFactory_PrefersCustomRegisteredRepository()
    {
        var custom = new StubCustomRepository();
        var sp = new ServiceCollection()
            .AddSingleton<IRepository<TestEntity, Guid>>(custom)
            .BuildServiceProvider();
        var factory = new DefaultEfRepositoryFactory(sp);

        using var harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        var repo = factory.Create<TestEntity, Guid>(harness.Context);

        repo.Should().BeSameAs(custom);
    }

    [Fact]
    public void DefaultFactory_WithoutCustomRepository_BuildsEfRepository()
    {
        var sp = new ServiceCollection().BuildServiceProvider();
        var factory = new DefaultEfRepositoryFactory(sp);

        using var harness = new SqliteHarness<TestDbContext>(o => new TestDbContext(o));
        var repo = factory.Create<TestEntity, Guid>(harness.Context);

        repo.Should().BeOfType<EfRepository<TestEntity, Guid>>();
    }

    private sealed class StubCustomRepository : EfRepository<TestEntity, Guid>
    {
        public StubCustomRepository() : base(new TestDbContext(
            new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("stub").Options))
        { }
    }
}
