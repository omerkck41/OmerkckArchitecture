using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using FluentValidation;
using MediatR;
using ValidationException = Core.CrossCuttingConcerns.GlobalException.Exceptions.ValidationException;

namespace Core.Application.Validation.Behaviors;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(result => result.Errors).Where(failure => failure != null).ToList();

            if (failures.Any())
            {
                var errors = failures
                    .GroupBy(x => x.PropertyName)
                    .Select(group => new ValidationExceptionModel
                    {
                        Property = group.Key,
                        Errors = group.Select(e => e.ErrorMessage),
                        ErrorCode = group.First().ErrorCode
                    }).ToList();

                throw new ValidationException(errors);
            }
        }

        return await next();
    }
}