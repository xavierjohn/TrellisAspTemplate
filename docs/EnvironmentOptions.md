# EnvironmentOptions Configuration Guide

## Overview

`EnvironmentOptions` provides a consistent way to configure environment-specific and region-specific settings for Azure resource naming and application configuration.

## Configuration Hierarchy

The configuration follows ASP.NET Core's hierarchical configuration system:

```
1. appsettings.json (base defaults)
2. appsettings.{Environment}.json (environment-specific)
3. Environment Variables (deployment/region/cloud-specific)
4. Command Line Arguments (optional overrides)
```

## Configuration Strategy

### Base Configuration (`appsettings.json`)
Contains **service-level** settings that rarely change:
- `ServiceName`: Application/service identifier (e.g., "BWF")

```json
{
  "EnvironmentOptions": {
    "ServiceName": "BWF"
  }
}
```

### Environment-Specific Configuration (`appsettings.{Environment}.json`)
Contains settings that vary by **environment** (local, test, ppe, prod):
- `Environment`: Environment type ("local", "test", "ppe", "prod")

**Examples:**
- `appsettings.Development.json` ? `"Environment": "local"`
- `appsettings.Test.json` ? `"Environment": "test"`
- `appsettings.Ppe.json` ? `"Environment": "ppe"`
- `appsettings.Production.json` ? `"Environment": "prod"`

### Deployment-Specific Configuration (Environment Variables)
Settings that vary by **deployment region and cloud**:
- `EnvironmentOptions__Cloud`: Azure cloud type (e.g., "AzureCloud", "AzureUSGovernment", "AzureChinaCloud")
- `EnvironmentOptions__Region`: Azure region (e.g., "westus2", "eastus2", "usgovvirginia")
- `EnvironmentOptions__RegionShortName`: Abbreviated region (e.g., "usw2", "use2", "usgv")

**Why Environment Variables?**
- **Multi-Cloud Support**: Deploy to different national clouds without code changes
- **BCDR Flexibility**: Each environment typically has multiple regional deployments
- **Deployment Portability**: Same Docker image/deployment package works across regions and clouds
- **Security Compliance**: Sovereign cloud deployments (Government, China) without configuration changes
- **Infrastructure Agnostic**: Easy to configure in Azure App Service, Container Apps, AKS, etc.

## Deployment Examples

### Development (Local)
Set in `launchSettings.json` or user secrets:
```json
{
  "EnvironmentOptions__Cloud": "AzureCloud",
  "EnvironmentOptions__Region": "westus2",
  "EnvironmentOptions__RegionShortName": "usw2"
}
```

### Azure App Service
Configure in Application Settings:
```
EnvironmentOptions__Cloud = AzureCloud
EnvironmentOptions__Region = westus2
EnvironmentOptions__RegionShortName = usw2
```

### Azure Container Apps / AKS
Set in environment variables:
```yaml
env:
  - name: EnvironmentOptions__Cloud
    value: "AzureCloud"
  - name: EnvironmentOptions__Region
    value: "westus2"
  - name: EnvironmentOptions__RegionShortName
    value: "usw2"
```

### Docker / Docker Compose
```yaml
environment:
  - EnvironmentOptions__Cloud=AzureCloud
  - EnvironmentOptions__Region=westus2
  - EnvironmentOptions__RegionShortName=usw2
```

## Resource Naming Convention

With these settings, resources are named consistently:

### Regional Resources
Format: `{environment}-{servicename}-{regionshortname}-{resourcetype}`

Example with Environment="test", ServiceName="BWF", RegionShortName="usw2":
- App Service: `test-bwf-usw2-app`
- Key Vault: `test-bwf-usw2-kv`
- Managed Identity: `test-bwf-usw2-id`

### Shared Resources
Format: `{environment}-{servicename}-{resourcetype}`

Example with Environment="test", ServiceName="BWF":
- Storage Account: `testbwfst` (sanitized for storage naming rules)
- Cosmos DB: `test-bwf-cosno`
- Service Bus: `test-bwf-sbns`

## National Cloud Deployment Examples

### Azure Commercial Cloud (Public)
```bash
EnvironmentOptions__Cloud=AzureCloud
EnvironmentOptions__Region=westus2
EnvironmentOptions__RegionShortName=usw2
```
**Endpoints:**
- Storage: `https://testbwfst.blob.core.windows.net`
- Cosmos DB: `https://test-bwf-cosno.documents.azure.com:443/`
- Service Bus: `test-bwf-sbns.servicebus.windows.net`

### Azure Government (US)
```bash
EnvironmentOptions__Cloud=AzureUSGovernment
EnvironmentOptions__Region=usgovvirginia
EnvironmentOptions__RegionShortName=usgv
```
**Endpoints:**
- Storage: `https://testbwfst.blob.core.usgovcloudapi.net`
- Cosmos DB: `https://test-bwf-cosno.documents.azure.us:443/`
- Service Bus: `test-bwf-sbns.servicebus.usgovcloudapi.net`

### Azure China Cloud
```bash
EnvironmentOptions__Cloud=AzureChinaCloud
EnvironmentOptions__Region=chinaeast2
EnvironmentOptions__RegionShortName=cnea2
```
**Endpoints:**
- Storage: `https://testbwfst.blob.core.chinacloud.cn`
- Cosmos DB: `https://test-bwf-cosno.documents.azure.cn:443/`
- Service Bus: `test-bwf-sbns.servicebus.chinacloud.cn`

### Azure Germany Cloud
```bash
EnvironmentOptions__Cloud=AzureGermanCloud
EnvironmentOptions__Region=germanycentral
EnvironmentOptions__RegionShortName=dec
```
**Endpoints:**
- Storage: `https://testbwfst.blob.core.cloudapi.de`
- Cosmos DB: `https://test-bwf-cosno.documents.azure.com:443/`
- Service Bus: `test-bwf-sbns.servicebus.cloudapi.de`

## BCDR Scenario Example

**Production Environment with Multi-Region Deployment:**

### Azure Commercial - West US 2 (Primary)
```bash
ASPNETCORE_ENVIRONMENT=Production
EnvironmentOptions__Cloud=AzureCloud
EnvironmentOptions__Region=westus2
EnvironmentOptions__RegionShortName=usw2
```
Generates: `prod-bwf-usw2-app`, `prod-bwf-usw2-kv`

### Azure Commercial - East US 2 (DR)
```bash
ASPNETCORE_ENVIRONMENT=Production
EnvironmentOptions__Cloud=AzureCloud
EnvironmentOptions__Region=eastus2
EnvironmentOptions__RegionShortName=use2
```
Generates: `prod-bwf-use2-app`, `prod-bwf-use2-kv`

Both deployments share: `prod-bwf-st`, `prod-bwf-cosno` (with correct cloud endpoints)

## Sovereign Cloud Deployment Example

**Government Compliance Scenario:**

### Azure Government - Virginia (Primary)
```bash
ASPNETCORE_ENVIRONMENT=Production
EnvironmentOptions__Cloud=AzureUSGovernment
EnvironmentOptions__Region=usgovvirginia
EnvironmentOptions__RegionShortName=usgv
```

### Azure Government - Texas (DR)
```bash
ASPNETCORE_ENVIRONMENT=Production
EnvironmentOptions__Cloud=AzureUSGovernment
EnvironmentOptions__Region=usgovtexas
EnvironmentOptions__RegionShortName=usgt
```

**Same application code, different cloud endpoints automatically configured!**

## Using in Code

### Dependency Injection
```csharp
public class MyService
{
    private readonly EnvironmentOptions _options;

    public MyService(IOptions<EnvironmentOptions> options)
    {
        _options = options.Value;
    }

    public string GetStorageUrl()
    {
        // Extension methods automatically use correct cloud endpoint
        return _options.GetBlobStorageSharedUrl();
        // Returns correct URL based on Cloud setting:
        // - AzureCloud: *.blob.core.windows.net
        // - AzureUSGovernment: *.blob.core.usgovcloudapi.net
        // - AzureChinaCloud: *.blob.core.chinacloud.cn
    }
}
```

### During Application Startup
```csharp
// Available during DI setup
var serviceName = Program.EnvironmentOptions.ServiceName;
var region = Program.EnvironmentOptions.Region;
var cloud = Program.EnvironmentOptions.Cloud;
```

## Best Practices

1. ? **DO** use `appsettings.json` for service-level settings (ServiceName)
2. ? **DO** use `appsettings.{Environment}.json` for environment-specific settings
3. ? **DO** use environment variables for cloud and region settings
4. ? **DO** use the same Docker image across all regions and clouds
5. ? **DO** test against correct cloud endpoints in non-production
6. ? **DON'T** hardcode cloud types or regions in configuration files
7. ? **DON'T** mix environment, region, and cloud settings
8. ? **DON'T** store secrets in any appsettings files (use Key Vault/secrets)
9. ? **DON'T** assume cloud endpoints - always use extension methods

## Supported Cloud Types

See `Acl/src/EnvironmentOptionsExts/CloudType.cs` for all supported values:

| Cloud Type | Description | Common Regions |
|------------|-------------|----------------|
| `AzureCloud` | Azure Commercial (Public) | westus2, eastus2, westeurope |
| `AzureUSGovernment` | Azure Government (US) | usgovvirginia, usgovtexas, usgovarizona |
| `AzureChinaCloud` | Azure China (21Vianet) | chinaeast2, chinanorth2 |
| `AzureGermanCloud` | Azure Germany | germanycentral, germanynortheast |

## Testing Different Configurations

### Test Environment - Azure Commercial
```bash
export ASPNETCORE_ENVIRONMENT=Test
export EnvironmentOptions__Cloud=AzureCloud
export EnvironmentOptions__Region=westus2
export EnvironmentOptions__RegionShortName=usw2
dotnet run --project Api/src
```

### Test Environment - Azure Government
```bash
export ASPNETCORE_ENVIRONMENT=Test
export EnvironmentOptions__Cloud=AzureUSGovernment
export EnvironmentOptions__Region=usgovvirginia
export EnvironmentOptions__RegionShortName=usgv
dotnet run --project Api/src
```

## Azure Region Short Names Reference

### Commercial Cloud Regions

| Region | Short Name |
|--------|-----------|
| West US 2 | usw2 |
| East US 2 | use2 |
| Central US | usc |
| North Central US | usnc |
| South Central US | ussc |
| West Europe | euw |
| North Europe | eun |
| UK South | uks |
| Southeast Asia | asse |
| East Asia | asea |

### Government Cloud Regions

| Region | Short Name |
|--------|-----------|
| US Gov Virginia | usgv |
| US Gov Texas | usgt |
| US Gov Arizona | usga |
| US DoD East | usgde |
| US DoD Central | usgdc |

### China Cloud Regions

| Region | Short Name |
|--------|-----------|
| China East 2 | cnea2 |
| China North 2 | cnno2 |

## Related Files

- `Acl/src/EnvironmentOptions.cs` - Class definition
- `Acl/src/EnvironmentOptionsExts/CloudType.cs` - Supported cloud types
- `Acl/src/EnvironmentOptionsExts/` - Extension methods for resource naming (cloud-aware)
- `Api/src/Program.cs` - Loads configuration at startup
- `Api/src/appsettings.*.json` - Environment-specific configuration
