namespace BestWeatherForecast.AntiCorruptionLayer;

public static class EnvironmentOptionsExt
{
    public static string GetResourceName(this EnvironmentOptions settings, string resourceType) => $"{settings.Environment}-{settings.RegionShortName}-{resourceType}-{settings.ServiceName}".ToLowerInvariant();
    public static string GetResourceNameShared(this EnvironmentOptions settings, string resourceType) => $"{settings.Environment}-{resourceType}-{settings.ServiceName}".ToLowerInvariant();
}
