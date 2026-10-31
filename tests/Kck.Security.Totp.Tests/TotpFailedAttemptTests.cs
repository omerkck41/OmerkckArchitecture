using AwesomeAssertions;
using Kck.Testing;
using Microsoft.Extensions.Caching.Memory;
using OtpNet;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Security.Totp.Tests;

/// <summary>Opt-in limit on failed codes per secret (TotpOptions.MaxFailedAttempts).</summary>
public sealed class TotpFailedAttemptTests : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public void Dispose() => _cache.Dispose();

    private TotpMfaProvider Provider(int maxFailedAttempts) =>
        new(new StaticOptionsMonitor<TotpOptions>(new TotpOptions { MaxFailedAttempts = maxFailedAttempts }), _cache);

    private static OtpNet.Totp Totp(string secret) => new(Base32Encoding.ToBytes(secret), mode: OtpHashMode.Sha256);

    // A code outside the verification window (previous, current and next step).
    private static string WrongCode(string secret)
    {
        var totp = Totp(secret);
        var valid = new[] { -30, 0, 30 }.Select(s => totp.ComputeTotp(DateTime.UtcNow.AddSeconds(s))).ToHashSet();
        return Enumerable.Range(0, 10).Select(i => $"{i}{i}{i}{i}{i}{i}").First(code => !valid.Contains(code));
    }

    private static async Task FailAsync(TotpMfaProvider provider, string secret, int times)
    {
        for (var i = 0; i < times; i++)
            (await provider.ValidateCodeAsync(secret, WrongCode(secret))).Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCodeAsync_LimitReached_RejectsTheCorrectCode()
    {
        var provider = Provider(maxFailedAttempts: 3);
        var secret = (await provider.GenerateSetupAsync("user", "app")).SecretKey;
        await FailAsync(provider, secret, 3);

        (await provider.ValidateCodeAsync(secret, Totp(secret).ComputeTotp())).Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCodeAsync_NoLimitByDefault_AcceptsTheCorrectCodeAfterFailures()
    {
        var provider = Provider(maxFailedAttempts: 0);
        var secret = (await provider.GenerateSetupAsync("user", "app")).SecretKey;
        await FailAsync(provider, secret, 10);

        (await provider.ValidateCodeAsync(secret, Totp(secret).ComputeTotp())).Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCodeAsync_Success_ResetsTheCount()
    {
        var provider = Provider(maxFailedAttempts: 3);
        var secret = (await provider.GenerateSetupAsync("user", "app")).SecretKey;
        // Two codes of different steps, both inside the window even if the step changes during the test.
        var now = DateTime.UtcNow;
        var (current, next) = (Totp(secret).ComputeTotp(now), Totp(secret).ComputeTotp(now.AddSeconds(30)));
        await FailAsync(provider, secret, 2);
        (await provider.ValidateCodeAsync(secret, current)).Should().BeTrue();
        await FailAsync(provider, secret, 2);

        (await provider.ValidateCodeAsync(secret, next)).Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCodeAsync_ParallelWrongCodes_LockTheCorrectCodeOut()
    {
        var provider = Provider(maxFailedAttempts: 3);
        var secret = (await provider.GenerateSetupAsync("user", "app")).SecretKey;
        var wrong = WrongCode(secret);

        await Task.WhenAll(Enumerable.Range(0, 64).Select(_ => Task.Run(() => provider.ValidateCodeAsync(secret, wrong))));

        (await provider.ValidateCodeAsync(secret, Totp(secret).ComputeTotp())).Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, 300)]
    [InlineData(3, 0)]
    [InlineData(3, -1)]   // Timeout.InfiniteTimeSpan
    [InlineData(3, 86401)]
    public void AddKckTotp_InvalidFailedAttemptSettings_FailsValidation(int maxFailedAttempts, int windowSeconds)
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddKckTotp(o =>
        {
            o.MaxFailedAttempts = maxFailedAttempts;
            o.FailedAttemptWindow = windowSeconds == -1 ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(windowSeconds);
        });
        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<TotpOptions>>().Value;

        act.Should().Throw<Microsoft.Extensions.Options.OptionsValidationException>();
    }

    [Fact]
    public async Task ValidateCodeAsync_LimitReached_DoesNotAffectAnotherSecret()
    {
        var provider = Provider(maxFailedAttempts: 3);
        var locked = (await provider.GenerateSetupAsync("a", "app")).SecretKey;
        var other = (await provider.GenerateSetupAsync("b", "app")).SecretKey;
        await FailAsync(provider, locked, 3);

        (await provider.ValidateCodeAsync(other, Totp(other).ComputeTotp())).Should().BeTrue();
    }
}
