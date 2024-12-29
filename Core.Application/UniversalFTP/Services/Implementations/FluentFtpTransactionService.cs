using Core.Application.UniversalFTP.Services.Models;

namespace Core.Application.UniversalFTP.Services.Implementations;

public class FluentFtpTransactionService
{
    private readonly TransactionContext _transactionContext;
    private readonly FluentFtpService _ftpService;

    public FluentFtpTransactionService(TransactionContext transactionContext, FluentFtpService ftpService)
    {
        _transactionContext = transactionContext;
        _ftpService = ftpService;
    }

    public void AddUploadOperation(string localPath, string remotePath)
    {
        _transactionContext.AddOperation(
            async () => await _ftpService.UploadFileAsync(localPath, remotePath),
            async () => await _ftpService.DeleteFileAsync(remotePath) // Rollback işlemi
        );
    }

    public void AddDeleteOperation(string remotePath)
    {
        _transactionContext.AddOperation(
            async () => await _ftpService.DeleteFileAsync(remotePath),
            async () => await _ftpService.UploadFileAsync("backup_path", remotePath) // Rollback için yedekleme
        );
    }
}