using Kck.Core.Abstractions.Pipeline;
using Kck.Persistence.Abstractions.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.MediatR.Behaviors;

/// <summary>
/// Wraps handler execution in a database transaction for requests implementing <see cref="ITransactionalRequest"/>.
/// Commits on success, rolls back on exception.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactionalRequest
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        Log.TransactionStarted(logger, requestName);

        try
        {
            var response = await next(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            Log.TransactionCommitted(logger, requestName);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            Log.TransactionRolledBack(logger, requestName);
            throw;
        }
    }
}
