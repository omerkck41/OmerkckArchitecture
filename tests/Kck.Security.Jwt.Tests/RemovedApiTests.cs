using AwesomeAssertions;
using Kck.Security.Abstractions.Token;
using Kck.Security.Jwt;
using Xunit;

namespace Kck.Security.Jwt.Tests;

/// <summary>
/// v4 (ADR-0028 K2): GetClaimsFromToken (KCK0001) read claims without verifying the signature and is removed.
/// </summary>
public sealed class RemovedApiTests
{
    [Fact]
    public void GetClaimsFromToken_IsRemovedFromInterfaceAndImplementation()
    {
        typeof(ITokenService).GetMethod("GetClaimsFromToken").Should().BeNull();
        typeof(JwtTokenService).GetMethod("GetClaimsFromToken").Should().BeNull();
    }
}
