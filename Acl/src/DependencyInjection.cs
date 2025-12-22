namespace BestWeatherForecast.AntiCorruptionLayer;

using BestWeatherForecast.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class DependencyInjection
{
    public static IServiceCollection AddAntiCorruptionLayer(this IServiceCollection services, EnvironmentOptions environmentOptions)
    {
        services.AddSingleton(Options.Create(environmentOptions));
        services.AddSingleton<IWeatherForecastService, WeatherForecastService>();
        return services;
    }
}
