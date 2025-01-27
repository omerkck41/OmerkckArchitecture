using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ModelState == null || !context.ModelState.IsValid)
        {
            var errors = context.ModelState?
                .Where(ms => ms.Value?.Errors.Count > 0)
                .Select(ms => new
                {
                    ms.Key,
                    Errors = ms.Value!.Errors.Select(e => e.ErrorMessage)
                }).ToList();

            context.Result = new BadRequestObjectResult(new { errors });
            return;
        }

        await next();
    }
}