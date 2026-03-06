namespace Application.Tests.WeatherForecast;

using System.Threading.Tasks;
using BestWeatherForecast.Application.Abstractions;
using BestWeatherForecast.Domain;

internal class MockWeatherForecastService : IWeatherForecastService
{
    public ValueTask<Result<WeatherForecast>> GetWeatherForecast(ZipCode zipCode)
    {
        var today = new DateOnly(2023, 6, 6);
        var dailyTempratures = new DailyTemperature[]
        {
            new DailyTemperature(today, 10, "Sunny"),
            new DailyTemperature(today.AddDays(1), 20, "Cloudy"),
            new DailyTemperature(today.AddDays(2), 30, "Rainy"),
        };


        return ValueTask.FromResult(zipCode.Value switch
        {
            "98052" => Result.Success(new WeatherForecast(zipCode, dailyTempratures)),
            "75014" => Result.Success(new WeatherForecast(zipCode, dailyTempratures)),
            _ => Result.Failure<WeatherForecast>(Error.NotFound("No weather forecast found for the zip code.", instance: zipCode))
        });
    }
}
