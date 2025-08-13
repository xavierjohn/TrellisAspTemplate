namespace BestWeatherForecast.AntiCorruptionLayer;

public static class EnvironmentOptionsServiceBusExt
{
    public static string GetServiceBusName(this EnvironmentOptions settings) => settings.GetResourceNameShared("sbns");

    public static string GetServiceBusNamespace(this EnvironmentOptions settings)
    {
        if (settings.Cloud == CloudType.AzureCloud)
            return $"{GetServiceBusName(settings)}.servicebus.windows.net";
        if (settings.Cloud == CloudType.AzureUSGovernment)
            return $"{GetServiceBusName(settings)}.servicebus.usgovcloudapi.net";

        throw new NotSupportedException($"Cloud type '{settings.Cloud}' is not supported.");
    }
}
