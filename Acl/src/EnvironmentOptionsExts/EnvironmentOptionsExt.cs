namespace BestWeatherForecast.AntiCorruptionLayer;

public static class EnvironmentOptionsExt
{
    public static string GetResourceName(this EnvironmentOptions settings, string resourceType) =>
        $"{settings.Environment}-{settings.RegionShortName}-{settings.ServiceName}-{resourceType}".ToLowerInvariant();

    public static string GetResourceNameShared(this EnvironmentOptions settings, string resourceType) =>
        $"{settings.Environment}-{settings.ServiceName}-{resourceType}".ToLowerInvariant();
}
