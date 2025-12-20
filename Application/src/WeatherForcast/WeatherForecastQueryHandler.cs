namespace BestWeatherForecast.Application.WeatherForcast;

using System.Threading.Tasks;
using BestWeatherForecast.Application.Abstractions;
using BestWeatherForecast.Domain;
using Mediator;

public class WeatherForecastQueryHandler : IQueryHandler<WeatherForecastQuery, Result<WeatherForecast>>
{
    private readonly IWeatherForecastService _weatherForcastService;

    public WeatherForecastQueryHandler(IWeatherForecastService weatherForcastService) => _weatherForcastService = weatherForcastService;

    public async ValueTask<Result<WeatherForecast>> Handle(WeatherForecastQuery query, CancellationToken cancellationToken)
        => await _weatherForcastService.GetWeatherForecast(query.ZipCode);
}
