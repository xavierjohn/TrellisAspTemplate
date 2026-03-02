namespace BestWeatherForecast.Application.WeatherForcast;

using BestWeatherForecast.Domain;
using Mediator;

public class WeatherForecastQuery : IQuery<Result<WeatherForecast>>
{
    public ZipCode ZipCode { get; }

    public static Result<WeatherForecastQuery> TryCreate(ZipCode zipCode)
        => new WeatherForecastQuery(zipCode);

    private WeatherForecastQuery(ZipCode zipCode) => ZipCode = zipCode;
}
