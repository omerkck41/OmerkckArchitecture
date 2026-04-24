using FluentAssertions;
using Kck.Documents.Abstractions;
using Kck.Documents.ImageSharp;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Documents.ImageSharp.Tests;

public class ImageSharpDiRegistrationTests
{
    [Fact]
    public void AddKckDocumentsImageSharp_ShouldRegisterProcessor()
    {
        var services = new ServiceCollection();

        services.AddKckDocumentsImageSharp();

        using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IImageProcessor>();

        processor.Should().BeOfType<ImageSharpProcessor>();
    }

    [Fact]
    public void AddKckDocumentsImageSharp_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddKckDocumentsImageSharp();

        using var provider = services.BuildServiceProvider();
        var a = provider.GetRequiredService<IImageProcessor>();
        var b = provider.GetRequiredService<IImageProcessor>();

        a.Should().BeSameAs(b);
    }
}
