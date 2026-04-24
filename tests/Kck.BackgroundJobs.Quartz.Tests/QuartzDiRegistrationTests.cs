using FluentAssertions;
using Kck.BackgroundJobs.Abstractions;
using Kck.BackgroundJobs.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.BackgroundJobs.Quartz.Tests;

public class QuartzDiRegistrationTests
{
    [Fact]
    public void AddKckBackgroundJobsQuartz_ShouldRegisterJobScheduler()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckBackgroundJobsQuartz();

        using var provider = services.BuildServiceProvider();
        var scheduler = provider.GetRequiredService<IJobScheduler>();

        scheduler.Should().BeOfType<QuartzJobScheduler>();
    }

    [Fact]
    public void AddKckBackgroundJobsQuartz_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckBackgroundJobsQuartz();

        using var provider = services.BuildServiceProvider();
        var a = provider.GetRequiredService<IJobScheduler>();
        var b = provider.GetRequiredService<IJobScheduler>();

        a.Should().BeSameAs(b);
    }
}
