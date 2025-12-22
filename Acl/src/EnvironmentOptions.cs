namespace BestWeatherForecast.AntiCorruptionLayer;

/// <summary>
/// Configuration options for environment-specific settings used to generate Azure resource names
/// following naming conventions.
/// </summary>
public class EnvironmentOptions
{
    /// <summary>
    /// Gets or sets the service name abbreviation (e.g., "BWF" for BestWeatherForecast).
    /// Used as part of resource naming convention.
    /// </summary>
    public string ServiceName { get; set; } = "BWF";

    /// <summary>
    /// Gets or sets the Azure region (e.g., "westus2", "eastus").
    /// Used for region-specific resource naming and configuration.
    /// </summary>
    public string Region { get; set; } = "local";

    /// <summary>
    /// Gets or sets the abbreviated region name (e.g., "usw2" for West US 2).
    /// Used in resource names to keep them concise.
    /// </summary>
    public string RegionShortName { get; set; } = "local";

    /// <summary>
    /// Gets or sets the environment name (e.g., "local", "test", "ppe", "prod").
    /// See <see cref="EnvironmentType"/> for standard values.
    /// </summary>
    public string Environment { get; set; } = EnvironmentType.Test;

    /// <summary>
    /// Gets or sets the cloud type (e.g., "AzureCloud", "AzureUSGovernment").
    /// See <see cref="CloudType"/> for supported values.
    /// </summary>
    public string Cloud { get; set; } = CloudType.AzureCloud;
}
