using AwesomeAssertions;
using Kck.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Kck.AspNetCore.Tests;

public sealed class SessionAndCookieTests
{
    private sealed class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = [];

        public bool IsAvailable => true;
        public string Id => "fake";
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out byte[]? value) => _store.TryGetValue(key, out value);
    }

    private sealed record Person(string Name, int Age);

    private readonly SessionManager _session = new();
    private readonly CookieManager _cookies = new();

    // ---- SessionManager ----

    [Fact]
    public void Set_Get_RoundTripsComplexObject()
    {
        var session = new FakeSession();

        _session.Set(session, "p", new Person("Ada", 36));

        _session.Get<Person>(session, "p").Should().Be(new Person("Ada", 36));
    }

    [Fact]
    public void Get_MissingKey_ReturnsDefault()
    {
        _session.Get<Person>(new FakeSession(), "none").Should().BeNull();
        _session.Get<int>(new FakeSession(), "none").Should().Be(0);
    }

    [Fact]
    public void Exists_ReflectsPresenceOfKey()
    {
        var session = new FakeSession();
        _session.Exists(session, "k").Should().BeFalse();

        _session.Set(session, "k", 1);

        _session.Exists(session, "k").Should().BeTrue();
    }

    [Fact]
    public void Remove_DeletesOnlyThatKey()
    {
        var session = new FakeSession();
        _session.Set(session, "a", 1);
        _session.Set(session, "b", 2);

        _session.Remove(session, "a");

        _session.Exists(session, "a").Should().BeFalse();
        _session.Exists(session, "b").Should().BeTrue();
    }

    [Fact]
    public void Clear_RemovesEveryKey()
    {
        var session = new FakeSession();
        _session.Set(session, "a", 1);
        _session.Set(session, "b", 2);

        _session.Clear(session);

        session.Keys.Should().BeEmpty();
    }

    [Fact]
    public void SessionManager_NullSession_ThrowsArgumentNull()
    {
        Action[] acts =
        [
            () => _session.Set<int>(null!, "k", 1),
            () => _session.Get<int>(null!, "k"),
            () => _session.Remove(null!, "k"),
            () => _session.Clear(null!),
            () => _session.Exists(null!, "k")
        ];

        foreach (var act in acts) act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SessionManager_EmptyKey_ThrowsArgumentException()
    {
        var session = new FakeSession();
        Action[] acts =
        [
            () => _session.Set(session, "", 1),
            () => _session.Get<int>(session, ""),
            () => _session.Remove(session, ""),
            () => _session.Exists(session, "")
        ];

        foreach (var act in acts) act.Should().Throw<ArgumentException>();
    }

    // ---- CookieManager ----

    [Fact]
    public void GetDefaultOptions_EnforcesSecureDefaults()
    {
        var before = DateTimeOffset.UtcNow;

        var options = _cookies.GetDefaultOptions();

        options.HttpOnly.Should().BeTrue();
        options.Secure.Should().BeTrue();
        options.SameSite.Should().Be(SameSiteMode.Strict);
        options.Expires.Should().NotBeNull();
        options.Expires!.Value.Should().BeCloseTo(before.AddDays(7), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Set_NoOptions_WritesSecureCookieHeader()
    {
        var context = new DefaultHttpContext();

        _cookies.Set(context.Response, "sid", "abc");

        var header = context.Response.Headers.SetCookie.ToString();
        header.Should().Contain("sid=abc").And.Contain("httponly").And.Contain("secure").And.Contain("samesite=strict");
    }

    [Fact]
    public void Set_CustomOptions_OverridesDefaults()
    {
        var context = new DefaultHttpContext();

        _cookies.Set(context.Response, "sid", "abc", new CookieOptions { HttpOnly = false, SameSite = SameSiteMode.Lax });

        var header = context.Response.Headers.SetCookie.ToString();
        header.Should().Contain("samesite=lax").And.NotContain("httponly");
    }

    [Fact]
    public void Get_ReadsCookieFromRequest()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "sid=abc; other=1";

        _cookies.Get(context.Request, "sid").Should().Be("abc");
        _cookies.Get(context.Request, "missing").Should().BeNull();
    }

    [Fact]
    public void Remove_WritesExpiredCookie()
    {
        var context = new DefaultHttpContext();

        _cookies.Remove(context.Response, "sid");

        var header = context.Response.Headers.SetCookie.ToString();
        header.Should().Contain("sid=").And.Contain("expires=Thu, 01 Jan 1970");
    }

    [Fact]
    public void CookieManager_InvalidArguments_Throw()
    {
        var context = new DefaultHttpContext();
        Action[] nullArgs =
        [
            () => _cookies.Set(null!, "k", "v"),
            () => _cookies.Get(null!, "k"),
            () => _cookies.Remove(null!, "k")
        ];
        Action[] emptyKeys =
        [
            () => _cookies.Set(context.Response, "", "v"),
            () => _cookies.Get(context.Request, ""),
            () => _cookies.Remove(context.Response, "")
        ];

        foreach (var act in nullArgs) act.Should().Throw<ArgumentNullException>();
        foreach (var act in emptyKeys) act.Should().Throw<ArgumentException>();
    }
}
