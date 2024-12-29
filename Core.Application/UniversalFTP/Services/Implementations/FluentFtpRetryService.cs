using Core.Application.UniversalFTP.Helper;
using Core.Application.UniversalFTP.Services.Models;

namespace Core.Application.UniversalFTP.Services.Implementations;

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