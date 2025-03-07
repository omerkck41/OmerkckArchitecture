using Microsoft.AspNetCore.Http;

namespace Core.Api.ApiHelperLibrary.Helpers;

public static class LocationHelper
{
    public static string CreateLocationHeader(HttpRequest request, object id)
    {
        var baseUrl = $"{request.Scheme}://{request.Host}{request.Path}";
        return $"{baseUrl}/{id}";
    }
}