using AwesomeAssertions;
using Kck.Persistence.Abstractions.Auditing;
using Kck.Persistence.EntityFramework.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Configuration;

// A pooled context is created from the root provider: a scoped or transient IAuditUserProvider would throw (scope
// validation) or record the first user forever, so the builder rejects it; whichever call comes second throws.
public sealed class PooledAuditUserGuardTests
{
    [Theory]
    [InlineData(ServiceLifetime.Scoped, true)]
    [InlineData(ServiceLifetime.Scoped, false)]
    [InlineData(ServiceLifetime.Transient, true)]
    [InlineData(ServiceLifetime.Transient, false)]
    public void PoolAndAuditInterceptor_WithNonSingletonAuditUserProvider_SecondCallThrows(ServiceLifetime lifetime, bool poolFirst)
    {
        IServiceCollection services = new ServiceCollection();
        services.Add(new ServiceDescriptor(typeof(IAuditUserProvider), typeof(FixedUser), lifetime));

        var act = () => services.AddKckPersistence(p =>
        {
            if (poolFirst)
                p.UseEntityFrameworkPool<TestDbContext>(o => o.UseInMemoryDatabase("guard")).AddAuditInterceptor();
            else
                p.AddAuditInterceptor().UseEntityFrameworkPool<TestDbContext>(o => o.UseInMemoryDatabase("guard"));
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*IAuditUserProvider*singleton*")
            .WithMessage("*IHttpContextAccessor*");
    }

    [Fact]
    public async Task PoolAndAuditInterceptor_WithSingletonAuditUserProvider_RecordsTheUser()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IAuditUserProvider, FixedUser>();
        services.AddKckPersistence(p => p
            .UseEntityFrameworkPool<TestDbContext>(o => o.UseSqlite(connection), poolSize: 2)
            .AddAuditInterceptor());
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        await using var scope = provider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await ctx.Database.EnsureCreatedAsync();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };

        ctx.TestEntities.Add(entity);
        await ctx.SaveChangesAsync();

        entity.CreatedBy.Should().Be("system");
    }

    [Fact]
    public void NonPooledContext_WithScopedAuditUserProvider_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddScoped<IAuditUserProvider, FixedUser>();

        var act = () => services.AddKckPersistence(p => p
            .UseEntityFramework<TestDbContext>(o => o.UseInMemoryDatabase("scoped-ok"))
            .AddAuditInterceptor());

        act.Should().NotThrow();
    }

    private sealed class FixedUser : IAuditUserProvider
    {
        public string? CurrentUser => "system";
    }
}
