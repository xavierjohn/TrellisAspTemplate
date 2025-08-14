namespace AntiCorruptionLayer.Tests;
using BestWeatherForecast.AntiCorruptionLayer;

public class KeyVaultTests
{
    [Theory]
    [InlineData("local")]
    [InlineData("test")]
    public void Will_get_KeyVault_name(string env)
    {
        // Arrange
        EnvironmentOptions environmentOptions = new()
        {
            Environment = env,
            RegionShortName = "usw2",
            ServiceName = "bwf"
        };

        var expected = $"{env}-usw2-kv-bwf";

        // Act
        var actual = environmentOptions.GetKeyVaultName();

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(CloudType.AzureCloud, "https://ppe-usw2-kv-bwf.vault.azure.net/")]
    [InlineData(CloudType.AzureUSGovernment, "https://ppe-usw2-kv-bwf.vault.usgovcloudapi.net/")]
    [InlineData(CloudType.AzureChinaCloud, "https://ppe-usw2-kv-bwf.vault.azure.cn/")]
    [InlineData(CloudType.AzureGermanCloud, "https://ppe-usw2-kv-bwf.vault.microsoftazure.de/")]
    public void Will_get_keyvault_uri_for_Cloud(string cloudType, string expectedUri)
    {
        // Arrange
        EnvironmentOptions environmentOptions = new()
        {
            Environment = EnvironmentType.Ppe,
            RegionShortName = "usw2",
            ServiceName = "bwf",
            Cloud = cloudType
        };

        // Act
        var actualUri = environmentOptions.GetKeyVaultUri();

        // Assert
        actualUri.Should().Be(expectedUri);
    }
}
