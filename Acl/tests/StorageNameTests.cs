namespace AntiCorruptionLayer.Tests;

using BestWeatherForecast.AntiCorruptionLayer;
using Xunit;

public class StorageNameTests
{
    [Theory]
    [InlineData("local")]
    [InlineData("test")]
    public void Will_get_storage_name(string env)
    {
        // Arrange
        EnvironmentOptions environmentOptions = new()
        {
            Environment = env,
            RegionShortName = "usw2",
            ServiceName = "bwf"
        };

        var expected = $"{env}stbwf";

        // Act
        var actual = environmentOptions.GetStorageNameShared();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Will_get_blob_storage_url()
    {
        // Arrange
        EnvironmentOptions environmentOptions = new()
        {
            Environment = EnvironmentType.Test,
            RegionShortName = "usw2",
            ServiceName = "bwf"
        };

        var expectedUrl = $"https://teststbwf.blob.core.windows.net";

        // Act
        var actualUrl = environmentOptions.GetBlobStorageSharedUrl();

        // Assert
        actualUrl.Should().Be(expectedUrl);
    }

    [Fact]
    public void Will_throw_exception_name_too_long()
    {
        // Arrange
        EnvironmentOptions environmentOptions = new()
        {
            Environment = "alongenvironmentthatdoesnotexist",
            RegionShortName = "usw3",
            ServiceName = "aservicenamewithasuperlongname"
        };

        // Act
        Action act = () => environmentOptions.GetStorageNameShared();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Will_get_blob_url_for_fairfax()
    {
        // Arrange
        EnvironmentOptions environmentOptions = new()
        {
            Environment = EnvironmentType.Ppe,
            RegionShortName = "usw2",
            ServiceName = "bwf",
            Cloud = CloudType.AzureUSGovernment
        };

        var expectedUrl = $"https://ppestbwf.blob.core.usgovcloudapi.net";

        // Act
        var actualUrl = environmentOptions.GetBlobStorageSharedUrl();

        // Assert
        actualUrl.Should().Be(expectedUrl);

    }
}
