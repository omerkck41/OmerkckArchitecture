using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace Core.ToolKit.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IWebHostEnvironment _env;

    public ValidationFilter(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {

            var problemDetails = new ValidationProblemDetails(context.ModelState);

            // Üretim ortamında detayları kısıtlayabilirsiniz.
            if (_env.IsProduction())
            {
                problemDetails.Detail = "Doğrulama hatası oluştu.";
            }

            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }
        await next();
    }
}

/*
 public static IServiceCollection AddApiServices(this IServiceCollection services, bool addValidationFilter = true)
    {
        services.AddControllers(options =>
        {
            if (addValidationFilter)
            {
                options.Filters.Add<Filters.ValidationFilter>();
            }
        });

        return services;
    }
 */