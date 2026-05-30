using AwesomeAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.HealthChecks;

public class EfCoreHealthCheckTests
{
    // --- Builder extension: name + tag defaulting ---

    [Fact]
    public void AddKckEfCoreCheck_DefaultName_UsesLowercasedTypeName_AndReadyTag()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks().AddKckEfCoreCheck<TestDbContext>();

        var reg = services.BuildServiceProvider()
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value
            .Registrations.Single();

        reg.Name.Should().Be("testdbcontext");
        reg.Tags.Should().Contain("ready");
    }

    [Fact]
    public void AddKckEfCoreCheck_ExplicitName_IsUsed()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks().AddKckEfCoreCheck<TestDbContext>(name: "db-custom");

        var reg = services.BuildServiceProvider()
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value
            .Registrations.Single();

        reg.Name.Should().Be("db-custom");
    }

    [Fact]
    public void AddKckEfCoreCheck_ExplicitTags_AreUsed()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks().AddKckEfCoreCheck<TestDbContext>(tags: ["live"]);

        var reg = services.BuildServiceProvider()
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value
            .Registrations.Single();

        reg.Tags.Should().Contain("live").And.NotContain("ready");
    }

    // --- Check execution: Healthy vs Unhealthy ---

    [Fact]
    public async Task CheckHealth_WhenDatabaseReachable_ReportsHealthy()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestDbContext>(o => o.UseSqlite(connection));
            services.AddHealthChecks().AddKckEfCoreCheck<TestDbContext>();
            var sp = services.BuildServiceProvider();

            var report = await sp.GetRequiredService<HealthCheckService>().CheckHealthAsync();

            report.Status.Should().Be(HealthStatus.Healthy);
            report.Entries["testdbcontext"].Description.Should().Contain("reachable");
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task CheckHealth_WhenDatabaseUnreachable_ReportsUnhealthy()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        // Path under a non-existent directory — SQLite cannot open it.
        services.AddDbContext<TestDbContext>(o =>
            o.UseSqlite("DataSource=/__kck_nonexistent__/unreachable.db"));
        services.AddHealthChecks().AddKckEfCoreCheck<TestDbContext>();
        var sp = services.BuildServiceProvider();

        var report = await sp.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        report.Status.Should().Be(HealthStatus.Unhealthy);
    }
}
