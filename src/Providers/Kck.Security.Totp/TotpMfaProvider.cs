using System.Security.Cryptography;
using Kck.Security.Abstractions.Mfa;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OtpNet;

namespace Kck.Security.Totp;

/// <summary>TOTP-based MFA provider using SHA256.</summary>
public sealed class TotpMfaProvider : IMfaProvider
{
    private readonly TotpOptions _options;
    private readonly IMemoryCache _replayCache;

    public TotpMfaProvider(IOptionsMonitor<TotpOptions> options, IMemoryCache replayCache)
    {
        _options = options.CurrentValue;
        _replayCache = replayCache;
    }

    public Task<MfaSetupResult> GenerateSetupAsync(string accountName, string issuer, CancellationToken ct = default)
    {
        var secretBytes = RandomNumberGenerator.GetBytes(20);
        var secretKey = Base32Encoding.ToString(secretBytes);

        var uri = new OtpUri(OtpType.Totp, secretKey, accountName, issuer,
            algorithm: OtpHashMode.Sha256,
            digits: _options.CodeLength,
            period: _options.StepSeconds).ToString();

        return Task.FromResult(new MfaSetupResult
        {
            SecretKey = secretKey,
            ProvisioningUri = uri
        });
    }

    public Task<bool> ValidateCodeAsync(string secretKey, string code, CancellationToken ct = default)
    {
        var totp = new OtpNet.Totp(
            Base32Encoding.ToBytes(secretKey),
            step: _options.StepSeconds,
            totpSize: _options.CodeLength,
            mode: OtpHashMode.Sha256);

        var isValid = totp.VerifyTotp(code, out long timeStepMatched,
            new VerificationWindow(_options.VerificationWindow, _options.VerificationWindow));

        if (!isValid)
            return Task.FromResult(false);

        // Replay protection: same time step + secret can only be used once
        var secretHashBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(secretKey));
        var secretHash =
#if NET9_0_OR_GREATER
            Convert.ToHexStringLower(secretHashBytes);
#else
            Convert.ToHexString(secretHashBytes).ToLowerInvariant();
#endif
        var replayKey = $"totp:replay:{secretHash}:{timeStepMatched}";
        if (_replayCache.TryGetValue(replayKey, out _))
            return Task.FromResult(false);

        // Cache used code — TTL = (VerificationWindow + 1) * StepSeconds
        var ttl = TimeSpan.FromSeconds((_options.VerificationWindow + 1) * _options.StepSeconds);
        _replayCache.Set(replayKey, true, ttl);

        return Task.FromResult(true);
    }
}
