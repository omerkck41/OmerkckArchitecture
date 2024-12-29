using Core.Application.Caching.Services;
using MediatR;

namespace Core.Application.Caching.Behaviors;

public class CacheRemovingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICacheRemoverRequest
{
    private readonly ICacheService _cacheService;

    public CacheRemovingBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (!string.IsNullOrEmpty(request.CacheKey))
        {
            await _cacheService.RemoveAsync(request.CacheKey, cancellationToken);
        }

        return response;
    }
}