using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiKeyAttribute>>();

        if (!context.HttpContext.Request.Headers.TryGetValue("ApiKey", out var extractedApiKey))
        {
            logger.LogWarning("ApiKey header is missing.");
            context.Result = new UnauthorizedResult();
            return;
        }

        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = await Task.Run(() => configuration.GetValue<string>("ApiSettings:ApiKey")); // Simulate async operation

        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogWarning("ApiKey is not configured in the application settings.");
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!apiKey.Equals(extractedApiKey))
        {
            logger.LogWarning("Invalid ApiKey provided.");
            context.Result = new UnauthorizedResult();
        }
    }

}