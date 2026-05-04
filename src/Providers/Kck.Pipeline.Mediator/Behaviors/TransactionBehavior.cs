using Kck.Core.Abstractions.Pipeline;
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
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        Log.TransactionStarted(logger, messageName);

        try
        {
            var response = await next(message, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            Log.TransactionCommitted(logger, messageName);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            Log.TransactionRolledBack(logger, messageName);
            throw;
        }
    }
}
