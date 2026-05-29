using System.Security.Cryptography;
using FluentAssertions;
using Kck.Security.Abstractions.Token;
using Kck.Security.Jwt;
using Kck.Security.Jwt.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Security.Jwt.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    private static string ValidKey()
    {
        using var rsa = RSA.Create(2048);
        return Convert.ToBase64String(rsa.ExportRSAPrivateKey());
    }

    [Fact]
    public void AddKckJwt_RegistersTokenServiceValidatorAndOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckJwt(o =>
        {
            o.Issuer = "issuer-x";
            o.Audience = "audience-x";
            o.AccessTokenExpiration = TimeSpan.FromMinutes(15);
            o.RefreshTokenTtlDays = 7;
            o.KeySource = RsaKeySource.Configuration;
            o.RsaKeyBase64 = ValidKey();
        });
        using var provider = services.BuildServiceProvider();

        provider.GetService<ITokenService>().Should().BeOfType<JwtTokenService>();
        provider.GetService<IValidateOptions<JwtOptions>>().Should().BeOfType<JwtOptionsValidator>();

        var options = provider.GetRequiredService<IOptionsMonitor<JwtOptions>>().CurrentValue;
        options.Issuer.Should().Be("issuer-x");
        options.Audience.Should().Be("audience-x");
    }

    [Fact]
    public void AddKckJwt_ReturnsSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddKckJwt(o => o.RsaKeyBase64 = ValidKey());

        result.Should().BeSameAs(services);
    }
}
