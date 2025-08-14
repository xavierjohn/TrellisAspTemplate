namespace BestWeatherForecast.AntiCorruptionLayer;

public static class EnvironmentOptionsExt
{
    // Region specific resources.
    // Example: App Service, Key Vault, Managed Identity. etc.
    public static string GetResourceName(this EnvironmentOptions settings, string resourceType) =>
        $"{settings.Environment}-{settings.RegionShortName}-{settings.ServiceName}-{resourceType}".ToLowerInvariant();

    // Shared resources.
    // Example: Storage Account, Cosmos DB, SQL etc.
    public static string GetResourceNameShared(this EnvironmentOptions settings, string resourceType) =>
        $"{settings.Environment}-{settings.ServiceName}-{resourceType}".ToLowerInvariant();
}
