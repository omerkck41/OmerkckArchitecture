using FluentValidation;
using Kck.Exceptions;
using Kck.Exceptions.Models;
using MediatR;
using ValidationException = Kck.Exceptions.ValidationException;

namespace Kck.Pipeline.MediatR.Behaviors;

/// <summary>
/// Runs FluentValidation validators before the handler executes.
/// Throws <see cref="ValidationException"/> if any validator fails.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

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

        return await next(cancellationToken);
    }
}
