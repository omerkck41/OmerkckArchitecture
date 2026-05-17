using Kck.Core.Abstractions.Results;
using Microsoft.AspNetCore.Mvc;

namespace KckWebApiTemplate.Features.WeatherForecast;

/// <summary>
/// Sample controller demonstrating Kck Result pattern.
/// Replace with your own controllers — this feature can be excluded via --no-sample.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class WeatherForecastController(IWeatherForecastService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WeatherForecast>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        var result = await service.GetForecastsAsync(ct);
        return result.Match<IActionResult>(
            onSuccess: forecasts => Ok(forecasts),
            onFailure: error => Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }
}
