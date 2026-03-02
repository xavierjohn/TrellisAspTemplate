namespace BestWeatherForecast.Domain;

using System.Collections.Generic;

public class WeatherForecast : Aggregate<ZipCode>
{
    private readonly List<DailyTemperature> dailyTemperatures = new();

    public IReadOnlyList<DailyTemperature> DailyTemperatures { get => dailyTemperatures; }

    public WeatherForecast(ZipCode zipCode, IEnumerable<DailyTemperature> dailyTemperatures) : base(zipCode)
        => this.dailyTemperatures.AddRange(dailyTemperatures);
}
