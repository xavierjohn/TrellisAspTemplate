namespace BestWeatherForecast.Application.Abstractions;

using Domain;

public interface IWeatherForecastService
{
    ValueTask<Result<WeatherForecast>> GetWeatherForecast(ZipCode zipCode);
}
