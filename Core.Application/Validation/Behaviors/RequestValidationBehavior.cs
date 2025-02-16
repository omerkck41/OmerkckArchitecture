using FluentValidation;
using MediatR;

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
            ValidationContext<TRequest> context = new(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            //var failures = validationResults
            //    .SelectMany(r => r.Errors)
            //    .Where(f => f != null)
            //    .ToList();

            var failtures = validationResults
                .SelectMany(result => result.Errors)
                .GroupBy(x => x.ErrorMessage)
                .Select(x => x.First())
                .Where(f => f != null)
                .ToList();

            // Hata varsa ValidationException fırlat
            if (failtures.Count > 0)
            {
                var errorDictionary = failtures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage).ToArray()
                    );

                throw new Core.CrossCuttingConcerns.GlobalException.Exceptions.ValidationException(errorDictionary);
            }
        }

        return await next();
    }
}