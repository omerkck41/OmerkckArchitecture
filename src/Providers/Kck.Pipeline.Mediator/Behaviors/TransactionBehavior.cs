using Kck.Pipeline.Abstractions;
using Kck.Persistence.Abstractions.UnitOfWork;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Wraps handler execution in a database transaction for requests implementing <see cref="ITransactionalRequest"/>.
/// </summary>
public sealed class TransactionBehavior<TMessage, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IRequest<TResponse>, ITransactionalRequest
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;
        await unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        Log.TransactionStarted(logger, messageName);

        try
        {
            var response = await next(message, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            Log.TransactionCommitted(logger, messageName);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
            Log.TransactionRolledBack(logger, messageName);
            throw;
        }
    }
}
