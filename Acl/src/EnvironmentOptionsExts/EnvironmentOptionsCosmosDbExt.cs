namespace BestWeatherForecast.AntiCorruptionLayer;

public static class EnvironmentOptionsCosmosDbExt
{
    public static string GetCosmosDbNameShared(this EnvironmentOptions settings) => settings.GetResourceNameShared("cosno");

    public static string GetCosmosDbNameSharedUrl(this EnvironmentOptions settings)
    {
        if (settings.Cloud == CloudType.AzureCloud)
            return $"https://{GetCosmosDbNameShared(settings)}.documents.azure.com:443/";

        throw new NotSupportedException($"Cloud type '{settings.Cloud}' is not supported.");
    }
}
