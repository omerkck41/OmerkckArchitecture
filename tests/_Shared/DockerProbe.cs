using System.IO.Pipes;
using System.Net.Sockets;

namespace Kck.Testing;

/// <summary>
/// Decides whether container tests (<see cref="DockerFactAttribute"/>, <see cref="DockerTheoryAttribute"/>) run or
/// are skipped. Deliberately free of Testcontainers: it only checks that a Docker daemon endpoint accepts a
/// connection.
/// </summary>
/// <remarks>
/// Endpoint: <c>DOCKER_HOST</c> when set (<c>tcp://host:port</c> and <c>npipe:////./pipe/name</c> are connected to,
/// <c>unix:///path</c> too); otherwise the Windows named pipe <c>\\.\pipe\docker_engine</c> on Windows, and on other
/// systems the first socket of <c>/var/run/docker.sock</c>, <c>$XDG_RUNTIME_DIR/docker.sock</c> (rootless) and
/// <c>~/.docker/run/docker.sock</c> (Docker Desktop for macOS) that accepts a connection. Where the runtime cannot
/// open Unix domain sockets, an existing socket file counts as reachable. A <c>DOCKER_HOST</c> scheme the probe
/// cannot check (for example <c>ssh://</c>) counts as reachable, so the tests run and report the real error instead of
/// hiding it; a malformed value counts as unreachable. The probe never throws. The result is computed once per test
/// process. <c>CI=true</c> or <c>KCK_REQUIRE_DOCKER=1</c> disables skipping altogether, so a pipeline without Docker
/// fails loudly.
/// </remarks>
internal static class DockerProbe
{
    internal const string SkipReason = "Docker daemon not reachable (set KCK_REQUIRE_DOCKER=1 to fail instead)";

    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(2);

    private static readonly Lazy<bool> CachedAvailability =
        new(() => IsAvailable(() => Environment.GetEnvironmentVariable("DOCKER_HOST")));

    /// <summary>The skip reason for the current process, or <see langword="null"/> when the tests must run.</summary>
    internal static string? CurrentSkipReason() =>
        GetSkipReason(Environment.GetEnvironmentVariable, () => CachedAvailability.Value);

    internal static string? GetSkipReason(Func<string, string?> getEnvironmentVariable, Func<bool> isDockerAvailable)
    {
        if (string.Equals(getEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase)
            || getEnvironmentVariable("KCK_REQUIRE_DOCKER") == "1")
        {
            return null;
        }

        return isDockerAvailable() ? null : SkipReason;
    }

    /// <summary>Probes the daemon endpoint described by <paramref name="getDockerHost"/> (the DOCKER_HOST value).</summary>
    internal static bool IsAvailable(Func<string?> getDockerHost)
    {
        try
        {
            return Probe(getDockerHost());
        }
        catch (Exception ex) when (IsProbeFailure(ex))
        {
            // The probe runs inside a Lazy during test discovery: a failure to reach the endpoint means "unreachable".
            return false;
        }
    }

    // Every exception a malformed DOCKER_HOST, a missing pipe/socket or a refused connection can raise. Anything else is
    // a real bug and propagates.
    private static bool IsProbeFailure(Exception ex) =>
        ex is System.Net.Sockets.SocketException
            or IOException
            or UnauthorizedAccessException
            or System.Security.SecurityException
            or ArgumentException
            or FormatException
            or InvalidOperationException
            or NotSupportedException
            or OperationCanceledException
            or TimeoutException;

    private static bool Probe(string? dockerHost)
    {
        if (string.IsNullOrWhiteSpace(dockerHost))
        {
            return OperatingSystem.IsWindows()
                ? CanConnectToPipe("docker_engine")
                : AnyUnixSocketReachable(DefaultUnixSocketCandidates());
        }

        if (!Uri.TryCreate(dockerHost, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme switch
        {
            "tcp" => CanConnectTcp(uri.Host, uri.Port),
            "npipe" => CanConnectToPipe(PipeName(uri)),
            "unix" => AnyUnixSocketReachable([uri.AbsolutePath]),
            _ => true,
        };
    }

    /// <summary>Whether any of <paramref name="socketPaths"/> is a Unix domain socket that accepts a connection.</summary>
    internal static bool AnyUnixSocketReachable(IEnumerable<string> socketPaths) =>
        socketPaths.Any(CanConnectUnixSocket);

    private static IEnumerable<string> DefaultUnixSocketCandidates()
    {
        yield return "/var/run/docker.sock";

        var runtimeDirectory = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        if (!string.IsNullOrWhiteSpace(runtimeDirectory))
            yield return Path.Join(runtimeDirectory, "docker.sock");

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(home))
            yield return Path.Join(home, ".docker", "run", "docker.sock");
    }

    private static string PipeName(Uri uri)
    {
        const string marker = "/pipe/";
        var path = uri.AbsolutePath;
        var index = path.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index >= 0 ? path[(index + marker.Length)..] : path.TrimStart('/');
    }

    private static bool CanConnectTcp(string host, int port)
    {
        if (port <= 0)
        {
            return false;
        }

        try
        {
            using var client = new TcpClient();
            using var timeout = new CancellationTokenSource(ConnectTimeout);
            client.ConnectAsync(host, port, timeout.Token).AsTask().GetAwaiter().GetResult();
            return client.Connected;
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException)
        {
            return false;
        }
    }

    private static bool CanConnectToPipe(string pipeName)
    {
        try
        {
            using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            pipe.Connect((int)ConnectTimeout.TotalMilliseconds);
            return true;
        }
        catch (Exception ex) when (ex is TimeoutException or IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool CanConnectUnixSocket(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            using var timeout = new CancellationTokenSource(ConnectTimeout);
            socket.ConnectAsync(new UnixDomainSocketEndPoint(path), timeout.Token).AsTask().GetAwaiter().GetResult();
            return socket.Connected;
        }
        catch (PlatformNotSupportedException)
        {
            // No Unix domain sockets on this runtime: the existing socket file is the best evidence available.
            return true;
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException or IOException)
        {
            return false;
        }
    }
}
