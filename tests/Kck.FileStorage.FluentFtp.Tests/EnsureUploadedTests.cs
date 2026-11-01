using AwesomeAssertions;
using FluentFTP;
using Kck.FileStorage.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.FileStorage.FluentFtp.Tests;

/// <summary>
/// Ö8: before 5.0 <c>UploadAsync</c> returned normally when FluentFTP reported <see cref="FtpStatus.Failed"/> (only a
/// warning was logged) or <see cref="FtpStatus.Skipped"/> (Overwrite = false and the file existed).
/// </summary>
public sealed class EnsureUploadedTests
{
    private const string Path = "backups/data.bin";

    [Fact]
    public void EnsureUploaded_Success_DoesNotThrow()
    {
        var act = () => FluentFtpService.EnsureUploaded(FtpStatus.Success, Path, overwrite: true);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureUploaded_Skipped_ThrowsIOExceptionNamingOverwrite()
    {
        var act = () => FluentFtpService.EnsureUploaded(FtpStatus.Skipped, Path, overwrite: false);

        act.Should().Throw<IOException>()
            .Which.Message.Should().Contain("Overwrite").And.Contain(Path);
    }

    [Fact]
    public void EnsureUploaded_Failed_ThrowsIOExceptionNamingThePath()
    {
        var act = () => FluentFtpService.EnsureUploaded(FtpStatus.Failed, Path, overwrite: true);

        act.Should().Throw<IOException>()
            .Which.Message.Should().Contain("failed").And.Contain(Path);
    }

    [Fact]
    public void EnsureUploaded_UnknownStatus_ThrowsIOException()
    {
        var act = () => FluentFtpService.EnsureUploaded((FtpStatus)42, Path, overwrite: true);

        act.Should().Throw<IOException>()
            .Which.Message.Should().Contain("failed").And.Contain(Path);
    }

    [Fact]
    public async Task AddKckFileStorageFluentFtp_RegistersSingletonServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckFileStorageFluentFtp(o => o.Host = "ftp.example.com");
        // FtpConnectionPool is IAsyncDisposable only, so the provider must be disposed asynchronously.
        await using var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<IFtpService>();
        var second = provider.GetRequiredService<IFtpService>();
        var storage = provider.GetRequiredService<IFileStorageService>();

        second.Should().BeSameAs(first);
        storage.Should().BeSameAs(first);
    }
}
