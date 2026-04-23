using Kck.Exceptions.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckBundleWebApiApplicationBuilderExtensions
{
    public static IApplicationBuilder UseKckWebApiDefaults(this IApplicationBuilder app)
    {
        app.UseKckExceptionHandling();
        app.UseKckAspNetCore();
        return app;
    }
}
