using FluentValidation;
using Kck.Exceptions;
using Kck.Exceptions.Models;
using Mediator;
using ValidationException = Kck.Exceptions.ValidationException;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Runs FluentValidation validators before the handler executes.
/// Throws <see cref="ValidationException"/> if any validator fails.
/// </summary>
public sealed class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(message, cancellationToken).ConfigureAwait(false);

        var context = new ValidationContext<TMessage>(message);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .Select(g => new ValidationExceptionModel
                {
                    Property = g.Key,
                    Errors = g.Select(e => e.ErrorMessage)
                })
                .ToList();

            throw new ValidationException(errors);
        }

        return await next(message, cancellationToken).ConfigureAwait(false);
    }
}
