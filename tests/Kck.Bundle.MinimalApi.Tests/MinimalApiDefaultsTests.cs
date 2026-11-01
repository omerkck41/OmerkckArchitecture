using AwesomeAssertions;
using Kck.Bundle.MinimalApi;
using Kck.Caching.Abstractions;
using Kck.Exceptions.AspNetCore.Handlers;
using Kck.Security.Abstractions.Token;
using Kck.Security.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Bundle.MinimalApi.Tests;

/// <summary>What <c>AddKckMinimalApiDefaults</c> registers, driven by the <c>Kck</c> configuration section.</summary>
public sealed class MinimalApiDefaultsTests
{
    private static ServiceCollection Register(params (string Key, string Value)[] settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)))
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckMinimalApiDefaults(configuration);
        return services;
    }

    [Fact]
    public void AddKckMinimalApiDefaults_WithoutConfig_RegistersCacheExceptionHandlerAndHealthChecks()
    {
        using var provider = Register().BuildServiceProvider();

        provider.GetService<ICacheService>().Should().NotBeNull();
        provider.GetService<GlobalExceptionHandler>().Should().NotBeNull();
        provider.GetService<HealthCheckService>().Should().NotBeNull();
    }

    [Fact]
    public void AddKckMinimalApiDefaults_WithoutJwtIssuer_DoesNotRegisterJwt()
    {
        var services = Register(("Kck:Security:Jwt:Audience", "api://orders"));

        services.Should().NotContain(d => d.ServiceType == typeof(ITokenService));
        services.Should().NotContain(d => d.ServiceType == typeof(IValidateOptions<JwtOptions>));
    }

    [Fact]
    public void AddKckMinimalApiDefaults_WithJwtIssuerAndAudience_BindsThemIntoJwtOptions()
    {
        var services = Register(
            ("Kck:Security:Jwt:Issuer", "https://issuer.example.com"),
            ("Kck:Security:Jwt:Audience", "api://orders"));
        // The bundle binds only Issuer and Audience; the signing key comes from the application.
        services.Configure<JwtOptions>(o => o.RsaKeyBase64 = "test-key-material");
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

        options.Issuer.Should().Be("https://issuer.example.com");
        options.Audience.Should().Be("api://orders");
        provider.GetService<ITokenService>().Should().BeOfType<JwtTokenService>();
    }

    [Fact]
    public void AddKckMinimalApiDefaults_WithJwtIssuerOnly_FailsValidationNamingAudience()
    {
        // Issuer alone registers the JWT service, but its options cannot validate: the start-up validation fails
        // with a message naming the missing Audience (and the missing signing key), instead of failing later.
        using var provider = Register(("Kck:Security:Jwt:Issuer", "https://issuer.example.com")).BuildServiceProvider();

        var resolve = () => provider.GetRequiredService<IOptions<JwtOptions>>().Value;

        resolve.Should().Throw<OptionsValidationException>()
            .WithMessage("*Audience: null or empty*")
            .WithMessage("*RsaKeyBase64: KeySource=Configuration but RsaKeyBase64 is empty*");
    }

    [Fact]
    public void Bundle_DoesNotReferenceMvcOrMediator()
    {
        var referenced = typeof(KckMinimalApiDefaults).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToList();
        var services = Register();

        referenced.Should().NotContain(n => n.StartsWith("Microsoft.AspNetCore.Mvc", StringComparison.Ordinal));
        referenced.Should().NotContain(n => n.Contains("Mediator", StringComparison.Ordinal));
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name)
            .Should().NotContain("Kck.Pipeline.Mediator");
        services.Should().NotContain(d =>
            d.ServiceType.Namespace != null && d.ServiceType.Namespace.StartsWith("Microsoft.AspNetCore.Mvc", StringComparison.Ordinal));
    }
}
