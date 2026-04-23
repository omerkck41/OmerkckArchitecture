using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Kck.AspNetCore.Helpers;

public static class LocationHelper
{
    public static string CreateLocationHeader(HttpRequest request, object id)
    {
        ArgumentNullException.ThrowIfNull(request);
        var baseUrl = request.GetEncodedUrl().TrimEnd('/');
        return $"{baseUrl}/{id}";
    }
}
