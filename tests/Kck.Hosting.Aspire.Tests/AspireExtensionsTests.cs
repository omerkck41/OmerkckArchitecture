using AwesomeAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Kck.Hosting.Aspire.Tests;

public sealed class AspireExtensionsTests
{
    [Fact]
    public void AddKckServiceDefaults_ShouldRegisterHealthChecks()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.AddKckServiceDefaults();

        var app = builder.Build();
        app.Services.GetService(typeof(HealthCheckService)).Should().NotBeNull();
    }

    [Fact]
    public void AddKckServiceDefaults_CalledTwice_ShouldNotThrow()
    {
        var builder = Host.CreateApplicationBuilder();

        var act = () =>
        {
            builder.AddKckServiceDefaults();
            builder.AddKckServiceDefaults();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void AddKckServiceDefaults_NullBuilder_ShouldThrowArgumentNull()
    {
        IHostApplicationBuilder builder = null!;

        var act = () => builder.AddKckServiceDefaults();

        act.Should().Throw<ArgumentNullException>();
    }
}
