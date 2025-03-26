using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Core.Api.ApiControllerBase.Helpers;

public static class LocationHelper
{
    /// <summary>
    /// Verilen request üzerinden, belirtilen id için Location header değeri oluşturur.
    /// </summary>
    public static string CreateLocationHeader(HttpRequest request, object id)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request), "HttpRequest cannot be null.");

        var baseUrl = request.GetEncodedUrl().TrimEnd('/');
        return $"{baseUrl}/{id}";
    }
}