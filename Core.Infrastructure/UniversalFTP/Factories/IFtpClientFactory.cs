using Core.Infrastructure.UniversalFTP.Services.Implementations;
using Core.Infrastructure.UniversalFTP.Services.Models;

namespace Core.Infrastructure.UniversalFTP.Factories;

public interface IFtpClientFactory
{
    FluentFtpService CreateFtpService(FtpSettings settings);
    FluentFtpDirectoryService CreateFtpDirectoryService(FtpSettings settings);
}