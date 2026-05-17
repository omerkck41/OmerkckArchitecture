using Kck.Pipeline.Abstractions;
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

        await unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        Log.TransactionStarted(logger, requestName);

        try
        {
            var response = await next(cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

            Log.TransactionCommitted(logger, requestName);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
            Log.TransactionRolledBack(logger, requestName);
            throw;
        }
    }
}
