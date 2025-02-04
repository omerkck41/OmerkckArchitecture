using Core.Infrastructure.UniversalFTP.Services.Models;
using MediatR;

namespace Core.Infrastructure.UniversalFTP.Behaviors;

public class FtpTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly TransactionContext _transactionContext;

    public FtpTransactionBehavior(TransactionContext transactionContext)
    {
        _transactionContext = transactionContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        await _transactionContext.CommitAsync(); // İşlemleri tamamla
        return response;
    }
}
