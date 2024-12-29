using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.API.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
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

            context.Result = new BadRequestObjectResult(new { success = false, errors });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}