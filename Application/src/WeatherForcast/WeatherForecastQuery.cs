namespace BestWeatherForecast.Application.WeatherForcast;

using BestWeatherForecast.Domain;
using FluentValidation;
using Mediator;

public class WeatherForecastQuery : IQuery<Result<WeatherForecast>>
{
    public ZipCode ZipCode { get; }

    public static Result<WeatherForecastQuery> TryCreate(ZipCode zipCode)
        => s_validator.ValidateToResult(new WeatherForecastQuery(zipCode));

    private WeatherForecastQuery(ZipCode zipCode) => ZipCode = zipCode;
    private static readonly InlineValidator<WeatherForecastQuery> s_validator = new()
    {
        v => v.RuleFor(x => x.ZipCode).NotNull(),
    };
}
