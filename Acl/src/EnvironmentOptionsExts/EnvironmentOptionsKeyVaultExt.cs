namespace BestWeatherForecast.AntiCorruptionLayer;

public static class EnvironmentOptionsKeyVaultExt
{
    public static string GetKeyVaultName(this EnvironmentOptions settings) => settings.GetResourceName("kv");

    public static string GetKeyVaultUri(this EnvironmentOptions settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var keyVaultName = settings.GetKeyVaultName();
        switch (settings.Cloud)
        {
            case CloudType.AzureCloud:
                return $"https://{keyVaultName}.vault.azure.net/";
            case CloudType.AzureUSGovernment:
                return $"https://{keyVaultName}.vault.usgovcloudapi.net/";
            default:
                throw new NotSupportedException($"Cloud type '{settings.Cloud}' is not supported for Key Vault URI generation.");
        }
    }
}
