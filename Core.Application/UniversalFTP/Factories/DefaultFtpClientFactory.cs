using Core.Application.UniversalFTP.Services.Implementations;
using Core.Application.UniversalFTP.Services.Models;

namespace Core.Application.UniversalFTP.Factories;

public class DefaultFtpClientFactory : IFtpClientFactory
{
    private readonly FtpConnectionPool _connectionPool;

    public DefaultFtpClientFactory(FtpConnectionPool connectionPool)
    {
        _connectionPool = connectionPool ?? throw new ArgumentNullException(nameof(connectionPool));
    }

    public FluentFtpService CreateFtpService(FtpSettings settings)
    {
        // Parametreyi kullanarak FluentFtpService oluşturma
        return new FluentFtpService(_connectionPool);
    }

    public FluentFtpDirectoryService CreateFtpDirectoryService(FtpSettings settings)
    {
        // Parametreyi kullanarak FluentFtpDirectoryService oluşturma
        return new FluentFtpDirectoryService(_connectionPool);
    }
}