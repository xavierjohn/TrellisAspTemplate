namespace BestWeatherForecast.Domain;

using System;
using System.Collections.Generic;

public class DailyTemperature : ValueObject
{
    public DateOnly Date { get; private set; }
    public double TemperatureC { get; private set; }

    public double TemperatureF => (TemperatureC * 9 / 5) + 32;

    public string Summary { get; private set; }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Date;
        yield return TemperatureC;
    }

    public DailyTemperature(DateOnly date, double temperatureC, string summary)
    {
        Date = date;
        TemperatureC = temperatureC;
        Summary = summary;
    }
}
