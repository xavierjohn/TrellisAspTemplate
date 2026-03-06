namespace Application.Tests;

using Application.Tests.WeatherForecast;
using BestWeatherForecast.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddMockAntiCorruptionLayer(this IServiceCollection services)
    {
        services.AddSingleton<IWeatherForecastService, MockWeatherForecastService>();
        return services;
    }
}
