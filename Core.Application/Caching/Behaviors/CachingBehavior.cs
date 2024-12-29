using Core.Application.Caching.Services;
using MediatR;

namespace Core.Application.Caching.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICachableRequest
{
    private readonly ICacheService _cacheService;

    public CachingBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!request.UseCache)
            return await next();

        var cacheKey = request.CacheKey;
        if (await _cacheService.ExistsAsync(cacheKey, cancellationToken))
        {
            var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
            if (cachedResponse is not null)
                return cachedResponse;
        }

        var response = await next();
        await _cacheService.SetAsync(cacheKey, response, request.CacheExpiration ?? TimeSpan.FromMinutes(30), cancellationToken);
        return response;
    }
}
