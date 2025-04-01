using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.Infrastructure.UniversalFTP.Services.Implementations;
using Core.Infrastructure.UniversalFTP.Services.Models;

namespace Core.Infrastructure.UniversalFTP.Factories;

public class DefaultFtpClientFactory : IFtpClientFactory
{
    private readonly FtpConnectionPool _connectionPool;

    public DefaultFtpClientFactory(FtpConnectionPool connectionPool)
    {
        _connectionPool = connectionPool ?? throw new CustomArgumentException(nameof(connectionPool));
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