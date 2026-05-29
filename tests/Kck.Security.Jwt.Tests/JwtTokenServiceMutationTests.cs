using System.Security.Claims;
using System.Security.Cryptography;
using AwesomeAssertions;
using Kck.Security.Abstractions.Token;
using Kck.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;
using NSubstitute;
using Xunit;

namespace Kck.Security.Jwt.Tests;

/// <summary>
/// Behaviour-distinguishing tests targeting mutants that survived the happy-path suite:
/// claim emission, issuer/audience/signing-key validation, error-result messages,
/// the exception catch path, File key-source loading, key-load failure handling and logging.
/// </summary>
public sealed class JwtTokenServiceMutationTests : IDisposable
{
    private readonly string _keyBase64;
    private readonly List<JwtTokenService> _created = [];

    public JwtTokenServiceMutationTests()
    {
        using var rsa = RSA.Create(2048);
        _keyBase64 = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
    }

    public void Dispose()
    {
        foreach (var s in _created)
            s.Dispose();
    }

    private static JwtOptions Options(string issuer = "test-issuer", string audience = "test-audience", string? keyBase64 = null) => new()
    {
        Issuer = issuer,
        Audience = audience,
        AccessTokenExpiration = TimeSpan.FromMinutes(15),
        KeySource = RsaKeySource.Configuration,
        RsaKeyBase64 = keyBase64
    };

    private JwtTokenService Build(JwtOptions opts, ILogger<JwtTokenService>? logger = null, ITokenBlacklistService? blacklist = null)
    {
        opts.RsaKeyBase64 ??= _keyBase64;
        var sut = new JwtTokenService(
            new StaticOptionsMonitor<JwtOptions>(opts),
            logger ?? NullLogger<JwtTokenService>.Instance,
            blacklist);
        _created.Add(sut);
        return sut;
    }

    private static TokenRequest Request(string userId = "user-1") =>
        new() { UserId = userId, Email = "test@test.com" };

    // --- Claim emission ----------------------------------------------------

    [Fact]
    public async Task CreateAndValidate_RoleRoundTripsIntoResult()
    {
        var sut = Build(Options());
        var token = await sut.CreateAccessTokenAsync(Request() with { Roles = ["Admin"] });

        var result = await sut.ValidateTokenAsync(token.AccessToken);

        result.IsValid.Should().BeTrue();
        result.Roles.Should().ContainSingle().Which.Should().Be("Admin");
    }

    [Fact]
    public async Task CreateAndValidate_UserIdAndEmailRoundTripIntoResult()
    {
        var sut = Build(Options());
        var token = await sut.CreateAccessTokenAsync(new TokenRequest { UserId = "u-42", Email = "me@example.com" });

        var result = await sut.ValidateTokenAsync(token.AccessToken);

        result.UserId.Should().Be("u-42");
        result.Email.Should().Be("me@example.com");
    }

    [Fact]
    public async Task CreateAccessToken_EmitsNameAndCustomClaims()
    {
        var sut = Build(Options());
        var token = await sut.CreateAccessTokenAsync(new TokenRequest
        {
            UserId = "u",
            Email = "e@e.com",
            Name = "Test User",
            CustomClaims = new Dictionary<string, string> { ["tenant"] = "acme" }
        });

        var jwt = new JsonWebToken(token.AccessToken);

        jwt.Claims.Should().Contain(c => c.Value == "Test User", "the Name claim must be emitted");
        jwt.Claims.Should().Contain(c => c.Type == "tenant" && c.Value == "acme");
    }

    // --- Validation parameters --------------------------------------------

    [Fact]
    public async Task ValidateToken_RejectsTokenWithWrongIssuer()
    {
        // Same signing key, different issuer: only ValidateIssuer can reject it.
        var signer = Build(Options(issuer: "other-issuer"));
        var token = await signer.CreateAccessTokenAsync(Request());

        var validator = Build(Options(issuer: "test-issuer"));
        var result = await validator.ValidateTokenAsync(token.AccessToken);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Token validation failed");
    }

    [Fact]
    public async Task ValidateToken_RejectsTokenWithWrongAudience()
    {
        var signer = Build(Options(audience: "other-audience"));
        var token = await signer.CreateAccessTokenAsync(Request());

        var validator = Build(Options(audience: "test-audience"));
        var result = await validator.ValidateTokenAsync(token.AccessToken);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateToken_RejectsTokenSignedWithDifferentKey()
    {
        using var otherRsa = RSA.Create(2048);
        var otherKey = Convert.ToBase64String(otherRsa.ExportRSAPrivateKey());

        var signer = Build(Options(keyBase64: otherKey));
        var token = await signer.CreateAccessTokenAsync(Request());

        var validator = Build(Options()); // expects the shared _keyBase64
        var result = await validator.ValidateTokenAsync(token.AccessToken);

        result.IsValid.Should().BeFalse();
    }

    // --- Exception catch path ---------------------------------------------

    [Fact]
    public async Task ValidateToken_MalformedToken_ReturnsFailureResult()
    {
        var sut = Build(Options());

        var result = await sut.ValidateTokenAsync("this-is-not-a-jwt");

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Token validation failed");
    }

    [Fact]
    public async Task ValidateToken_SigningKeyLoadFailure_ReturnsFailureViaCatchAndLogs()
    {
        // Building the validation parameters accesses the (lazily loaded) signing key,
        // which throws FormatException here — exercising the exception catch path.
        var logger = new CapturingLogger<JwtTokenService>();
        var sut = Build(Options(keyBase64: "!!!not-valid-base64!!!"), logger);

        var result = await sut.ValidateTokenAsync("a.b.c");

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Token validation failed");
        logger.Entries.Should().Contain(e => e.Contains("validation failed"));
    }

    // --- Logging -----------------------------------------------------------

    [Fact]
    public async Task CreateAccessToken_LogsCreationWithUserId()
    {
        var logger = new CapturingLogger<JwtTokenService>();
        var sut = Build(Options(), logger);

        await sut.CreateAccessTokenAsync(Request("user-67"));

        logger.Entries.Should().Contain(e => e.Contains("user-67"));
    }

    [Fact]
    public async Task ValidateToken_RevokedToken_LogsAndReturnsRevoked()
    {
        var logger = new CapturingLogger<JwtTokenService>();
        var blacklist = Substitute.For<ITokenBlacklistService>();
        blacklist.IsRevokedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var sut = Build(Options(), logger, blacklist);
        var token = await sut.CreateAccessTokenAsync(Request());

        var result = await sut.ValidateTokenAsync(token.AccessToken);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("revoked");
        logger.Entries.Should().Contain(e => e.Contains("revoked"));
    }

    // --- RSA key sources ---------------------------------------------------

    [Fact]
    public async Task LoadSigningKey_FileSource_ImportsPemAndSignsVerifiably()
    {
        var logger = new CapturingLogger<JwtTokenService>();
        using var rsa = RSA.Create(2048);
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, rsa.ExportRSAPrivateKeyPem());
        var sameKeyBase64 = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

        try
        {
            var opts = Options();
            opts.KeySource = RsaKeySource.File;
            opts.RsaKeyBase64 = null;
            opts.RsaKeyPath = path;
            // RsaKeyBase64 must stay null for the File branch; bypass the Build default.
            var fileSut = new JwtTokenService(new StaticOptionsMonitor<JwtOptions>(opts), logger);
            _created.Add(fileSut);

            var token = await fileSut.CreateAccessTokenAsync(Request());

            // Cross-validate with a Configuration service holding the SAME key: proves the PEM was
            // actually imported rather than signed with an auto-generated (un-imported) RSA key.
            var verifier = Build(Options(keyBase64: sameKeyBase64));
            var result = await verifier.ValidateTokenAsync(token.AccessToken);

            result.IsValid.Should().BeTrue();
            logger.Entries.Should().Contain(e => e.Contains("File"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadSigningKey_FileSource_MissingFile_Throws()
    {
        var opts = Options();
        opts.KeySource = RsaKeySource.File;
        opts.RsaKeyBase64 = null;
        opts.RsaKeyPath = Path.Join(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.pem");
        var sut = new JwtTokenService(new StaticOptionsMonitor<JwtOptions>(opts), NullLogger<JwtTokenService>.Instance);
        _created.Add(sut);

        var act = async () => await sut.CreateAccessTokenAsync(Request());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task LoadSigningKey_InvalidBase64_DisposesAndRethrows()
    {
        var opts = Options(keyBase64: "!!!not-valid-base64!!!");
        var sut = Build(opts);

        var act = async () => await sut.CreateAccessTokenAsync(Request());

        await act.Should().ThrowAsync<FormatException>();
    }

    // --- Test double -------------------------------------------------------

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add(formatter(state, exception));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
