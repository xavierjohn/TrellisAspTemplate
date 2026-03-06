namespace Api.Tests._2023_06_06;

public class DailyTemperature
{
    public DateOnly date { get; set; }

    public double temperatureC { get; set; }

    public double temperatureF { get; set; }

    public string summary { get; set; } = string.Empty;
}
