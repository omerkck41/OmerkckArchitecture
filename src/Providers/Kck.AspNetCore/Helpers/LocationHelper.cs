using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Kck.AspNetCore.Helpers;

/// <summary>
/// Utility class for building HTTP Location header values from the current request URL.
/// </summary>
public static class LocationHelper
{
    /// <summary>
    /// Constructs a Location header value by appending <paramref name="id"/> to the encoded base URL
    /// of the given <paramref name="request"/>.
    /// </summary>
    public static string CreateLocationHeader(HttpRequest request, object id)
    {
        ArgumentNullException.ThrowIfNull(request);
        var baseUrl = request.GetEncodedUrl().TrimEnd('/');
        return $"{baseUrl}/{id}";
    }
}
