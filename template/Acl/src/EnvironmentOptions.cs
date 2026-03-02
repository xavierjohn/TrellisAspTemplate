namespace BestWeatherForecast.AntiCorruptionLayer;

public class EnvironmentOptions
{
    public string ServiceName { get; set; } = "BWF";

    public string Region { get; set; } = "local";

    public string RegionShortName { get; set; } = "local";

    public string Environment { get; set; } = EnvironmentType.Test;

    public string Cloud { get; set; } = CloudType.AzureCloud;
}
