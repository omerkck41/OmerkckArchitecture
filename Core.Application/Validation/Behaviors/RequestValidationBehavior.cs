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

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // Hata varsa ValidationException fırlat
            if (failures.Count > 0)
            {
                // failures listesini Dictionary<string, string[]> formatına dönüştür
                var errorDictionary = failures
                    .GroupBy(f => f.PropertyName) // PropertyName'e göre grupla
                    .ToDictionary(
                        g => g.Key,                // Key olarak PropertyName kullan
                        g => g.Select(f => f.ErrorMessage).ToArray() // Hata mesajlarını dizi olarak al
                    );

                // ValidationException fırlat
                throw new ValidationException((IEnumerable<FluentValidation.Results.ValidationFailure>)errorDictionary);
            }
        }

        return await next();
    }
}