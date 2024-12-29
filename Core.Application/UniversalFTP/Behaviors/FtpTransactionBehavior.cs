using Core.Application.UniversalFTP.Services.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.UniversalFTP.Behaviors;

public class FtpTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<FtpTransactionBehavior<TRequest, TResponse>> _logger;
    private readonly TransactionContext _transactionContext;

    public FtpTransactionBehavior(
        ILogger<FtpTransactionBehavior<TRequest, TResponse>> logger,
        TransactionContext transactionContext)
    {
        _logger = logger;
        _transactionContext = transactionContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting FTP transaction for request: {Request}", typeof(TRequest).Name);

        try
        {
            var response = await next();

            await _transactionContext.CommitAsync(); // İşlemleri tamamla
            _logger.LogInformation("FTP transaction completed successfully for request: {Request}", typeof(TRequest).Name);
            return response;
        }
        catch (Exception ex)
        {
            await _transactionContext.RollbackAsync(); // Geri alma işlemlerini çalıştır
            _logger.LogError(ex, "FTP transaction failed for request: {Request}", typeof(TRequest).Name);
            throw;
        }
    }
}
