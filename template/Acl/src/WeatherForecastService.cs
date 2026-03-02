namespace BestWeatherForecast.AntiCorruptionLayer;

using System.Threading.Tasks;
using BestWeatherForecast.Application.Abstractions;
using BestWeatherForecast.Domain;

public class WeatherForecastService : IWeatherForecastService
{
    private static readonly string[] s_summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public ValueTask<Result<WeatherForecast>> GetWeatherForecast(ZipCode zipCode)
    {
        var dailyTempratures = Enumerable.Range(1, 5).Select(index => new DailyTemperature
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            s_summaries[Random.Shared.Next(s_summaries.Length)]
        )).ToArray();

        return ValueTask.FromResult(zipCode.Value switch
        {
            "98052" => Result.Success(new WeatherForecast(zipCode, dailyTempratures)),
            "75014" => Result.Success(new WeatherForecast(zipCode, dailyTempratures)),
            _ => Result.Failure<WeatherForecast>(Error.NotFound("No weather forecast found for the zip code.", instance: zipCode))
        });
    }
}
