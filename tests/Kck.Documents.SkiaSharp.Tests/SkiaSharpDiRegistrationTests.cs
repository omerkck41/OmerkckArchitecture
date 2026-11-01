using AwesomeAssertions;
using Kck.Documents.Abstractions;
using Kck.Documents.SkiaSharp.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Documents.SkiaSharp.Tests;

public class SkiaSharpDiRegistrationTests
{
    [Fact]
    public void AddKckDocumentsSkiaSharp_ShouldRegisterProcessor()
    {
        var services = new ServiceCollection();

        services.AddKckDocumentsSkiaSharp();

        using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IImageProcessor>();

        processor.Should().BeOfType<SkiaSharpImageProcessor>();
    }

    [Fact]
    public void AddKckDocumentsSkiaSharp_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddKckDocumentsSkiaSharp();

        using var provider = services.BuildServiceProvider();
        var a = provider.GetRequiredService<IImageProcessor>();
        var b = provider.GetRequiredService<IImageProcessor>();

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void AddKckDocumentsSkiaSharp_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();

        services.AddKckDocumentsSkiaSharp(o => o.MaxPixels = 1234);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<SkiaSharpOptions>>().Value.MaxPixels.Should().Be(1234);
    }

    [Fact]
    public void AddKckDocumentsSkiaSharp_WithoutConfigure_UsesDefaultMaxPixels()
    {
        var services = new ServiceCollection();

        services.AddKckDocumentsSkiaSharp();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<SkiaSharpOptions>>().Value.MaxPixels.Should().Be(50_000_000);
    }

    [Fact]
    public void AddKckDocumentsSkiaSharp_InvalidMaxPixels_FailsValidation()
    {
        var services = new ServiceCollection();

        services.AddKckDocumentsSkiaSharp(o => o.MaxPixels = 0);

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IOptions<SkiaSharpOptions>>().Value;

        act.Should().Throw<OptionsValidationException>().WithMessage("*MaxPixels*");
    }

    [Fact]
    public void AddKckDocumentsSkiaSharp_RegistersValidatorAndValidateOnStart()
    {
        var services = new ServiceCollection();

        services.AddKckDocumentsSkiaSharp();

        services.Should().Contain(d => d.ServiceType == typeof(IValidateOptions<SkiaSharpOptions>)
                                       && d.ImplementationType == typeof(SkiaSharpOptionsValidator));
        services.Should().Contain(d => d.ServiceType.Name.Contains("IStartupValidator"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void SkiaSharpOptionsValidator_NonPositiveMaxPixels_Fails(int maxPixels)
    {
        var result = new SkiaSharpOptionsValidator().Validate(null, new SkiaSharpOptions { MaxPixels = maxPixels });

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("MaxPixels").And.Contain(maxPixels.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50_000_000)]
    public void SkiaSharpOptionsValidator_PositiveMaxPixels_Succeeds(int maxPixels)
    {
        var result = new SkiaSharpOptionsValidator().Validate(null, new SkiaSharpOptions { MaxPixels = maxPixels });

        result.Succeeded.Should().BeTrue();
    }
}
