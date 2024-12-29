using Core.Application.UniversalFTP.Services.Implementations;
using Core.Application.UniversalFTP.Services.Models;

namespace Core.Application.UniversalFTP.Factories;

public interface IFtpClientFactory
{
    FluentFtpService CreateFtpService(FtpSettings settings);
    FluentFtpDirectoryService CreateFtpDirectoryService(FtpSettings settings);
}