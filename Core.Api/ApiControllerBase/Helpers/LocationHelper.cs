using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Core.Api.ApiControllerBase.Helpers;

public static class LocationHelper
{
    public static string CreateLocationHeader(HttpRequest request, object id)
    {
        if (request == null)
            throw new CustomException(nameof(request));

        var baseUrl = request.GetEncodedUrl().TrimEnd('/');
        return $"{baseUrl}/{id}";
    }
}