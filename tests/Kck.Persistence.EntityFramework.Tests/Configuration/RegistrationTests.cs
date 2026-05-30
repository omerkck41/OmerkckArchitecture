using AwesomeAssertions;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Kck.Persistence.EntityFramework.UnitOfWork;
using Microsoft.EntityFrameworkCore;
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
