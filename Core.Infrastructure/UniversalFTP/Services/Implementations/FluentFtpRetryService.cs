using Core.Infrastructure.UniversalFTP.Helper;
using Core.Infrastructure.UniversalFTP.Services.Models;

namespace Core.Infrastructure.UniversalFTP.Services.Implementations;

public class FluentFtpRetryService
{
    private readonly FluentFtpService _ftpService;

    public FluentFtpRetryService(FluentFtpService ftpService)
    {
        _ftpService = ftpService;
    }

    public async Task<FtpOperationResult> UploadFileWithRetryAsync(string localPath, string remotePath)
    {
        return await RetryHelper.RetryAsync(async () =>
        {
            return await _ftpService.UploadFileAsync(localPath, remotePath);
        }, 3, TimeSpan.FromSeconds(2));
    }
}