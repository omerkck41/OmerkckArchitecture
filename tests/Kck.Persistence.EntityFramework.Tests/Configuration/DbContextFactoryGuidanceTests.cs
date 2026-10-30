using AwesomeAssertions;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Configuration;

/// <summary>
/// UseEntityFramework registers the context per scope (AddDbContext); AddDbContextFactory for the same context registers
/// its options as a singleton. Together they fail with EF / DI errors that do not say what to do (factory first: the
/// scoped context cannot be resolved; factory after: a singleton consumes scoped options). Plan G4.5.
/// </summary>
public sealed class DbContextFactoryGuidanceTests
{
    [Fact]
    public void UseEntityFramework_AfterAddDbContextFactory_ThrowsWithGuidance()
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<TestDbContext>(o => o.UseInMemoryDatabase("factory-first"));

        var act = () => services.AddKckPersistence(p => p.UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase("factory-first")));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*UseEntityFramework<TestDbContext>*AddDbContextFactory<TestDbContext>*IServiceScopeFactory*UseEntityFrameworkPool*persistence.md*");
    }

    [Fact]
    public async Task UseEntityFramework_WithDbContextFactory_ThrowsWithGuidance()
    {
        // The factory registered after UseEntityFramework: reported when the first context is created (without
        // ValidateOnBuild; with it, the container reports its own lifetime error first, see docs/providers/persistence.md).
        var services = new ServiceCollection();
        services.AddKckPersistence(p => p.UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase("factory-after")));
        services.AddDbContextFactory<TestDbContext>(o => o.UseInMemoryDatabase("factory-after"));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var act = () => scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*UseEntityFramework<TestDbContext>*AddDbContextFactory<TestDbContext>*");
    }

    [Fact]
    public async Task UseEntityFramework_WithFactoryForAnotherContext_Works()
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<OtherDbContext>(o => o.UseInMemoryDatabase("other"));
        services.AddKckPersistence(p => p.UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase("own")));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = provider.CreateAsyncScope();

        scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Should().NotBeNull();
        (await provider.GetRequiredService<IDbContextFactory<OtherDbContext>>().CreateDbContextAsync()).Should().NotBeNull();
    }

    [Fact]
    public async Task UseEntityFramework_WithScopedDbContextFactory_Works()
    {
        // A scoped factory shares the scoped options: a valid combination, not reported.
        var services = new ServiceCollection();
        services.AddDbContextFactory<TestDbContext>(o => o.UseInMemoryDatabase("scoped-factory"), ServiceLifetime.Scoped);
        services.AddKckPersistence(p => p.UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase("scoped-factory")));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = provider.CreateAsyncScope();

        scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Should().NotBeNull();
        (await scope.ServiceProvider.GetRequiredService<IDbContextFactory<TestDbContext>>().CreateDbContextAsync()).Should().NotBeNull();
    }

    [Fact]
    public async Task UseEntityFramework_WithKeyedDbContextFactory_Works()
    {
        // A keyed factory (for example a read replica) does not touch the context's own options.
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IDbContextFactory<TestDbContext>, ReplicaFactory>("replica");
        services.AddKckPersistence(p => p.UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase("keyed")));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = provider.CreateAsyncScope();

        scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Should().NotBeNull();
    }

    internal sealed class OtherDbContext(DbContextOptions<OtherDbContext> options) : DbContext(options);

    internal sealed class ReplicaFactory : IDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext() =>
            new(new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("replica").Options);
    }
}
