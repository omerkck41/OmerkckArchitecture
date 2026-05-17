using Kck.Core.Abstractions.Results;

namespace KckWebApiTemplate.Features.WeatherForecast;

/// <summary>WeatherForecast model.</summary>
public sealed record WeatherForecast(DateOnly Date, int TemperatureC, string Summary)
{
    /// <summary>Temperature in Fahrenheit.</summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

/// <summary>Service contract for weather forecasts.</summary>
public interface IWeatherForecastService
{
    Task<Result<IReadOnlyList<WeatherForecast>>> GetForecastsAsync(CancellationToken ct = default);
}

/// <summary>
/// Sample implementation using the Kck Result pattern.
/// Replace this with a real data source (e.g. IReadRepository).
/// </summary>
public sealed class WeatherForecastService : IWeatherForecastService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public Task<Result<IReadOnlyList<WeatherForecast>>> GetForecastsAsync(CancellationToken ct = default)
    {
        IReadOnlyList<WeatherForecast> forecasts = Enumerable.Range(1, 5)
            .Select(i => new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]))
            .ToList()
            .AsReadOnly();

        return Task.FromResult(Result<IReadOnlyList<WeatherForecast>>.Success(forecasts));
    }
}
