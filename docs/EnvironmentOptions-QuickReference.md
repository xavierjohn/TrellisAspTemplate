# EnvironmentOptions Quick Reference

## Configuration Layers

| Setting | Where to Set | When to Change | Example Values |
|---------|-------------|----------------|----------------|
| **ServiceName** | `appsettings.json` | When renaming service | "BWF", "MyApp" |
| **Environment** | `appsettings.{Env}.json` | Per environment type | "local", "test", "ppe", "prod" |
| **Cloud** | Environment Variables | Per deployment cloud | "AzureCloud", "AzureUSGovernment" |
| **Region** | Environment Variables | Per deployment region | "westus2", "usgovvirginia" |
| **RegionShortName** | Environment Variables | Per deployment region | "usw2", "usgv" |

## Common Deployment Scenarios

### Scenario 1: Development (Local)
```bash
# appsettings.Development.json
{ "Environment": "local", "Region": "local", "RegionShortName": "local", "Cloud": "AzureCloud" }
```

### Scenario 2: Test - Multi-Region (Commercial Cloud)
```bash
# West US 2
EnvironmentOptions__Cloud=AzureCloud
EnvironmentOptions__Region=westus2
EnvironmentOptions__RegionShortName=usw2

# East US 2 (DR)
EnvironmentOptions__Cloud=AzureCloud
EnvironmentOptions__Region=eastus2
EnvironmentOptions__RegionShortName=use2
```

### Scenario 3: Production - Commercial Cloud
```bash
# Primary
EnvironmentOptions__Cloud=AzureCloud
EnvironmentOptions__Region=westus2
EnvironmentOptions__RegionShortName=usw2
```

### Scenario 4: Production - Government Cloud
```bash
# US Gov Virginia
EnvironmentOptions__Cloud=AzureUSGovernment
EnvironmentOptions__Region=usgovvirginia
EnvironmentOptions__RegionShortName=usgv
```

### Scenario 5: Production - China Cloud
```bash
# China East 2
EnvironmentOptions__Cloud=AzureChinaCloud
EnvironmentOptions__Region=chinaeast2
EnvironmentOptions__RegionShortName=cnea2
```

## Supported Cloud Types

| Cloud | Constant | Endpoints |
|-------|----------|-----------|
| Azure Commercial | `AzureCloud` | `*.windows.net`, `*.azure.com` |
| Azure US Government | `AzureUSGovernment` | `*.usgovcloudapi.net`, `*.azure.us` |
| Azure China | `AzureChinaCloud` | `*.chinacloud.cn`, `*.azure.cn` |
| Azure Germany | `AzureGermanCloud` | `*.cloudapi.de` |

## Resource Naming Examples

Given: ServiceName="BWF", Environment="prod", RegionShortName="usw2"

### Regional Resources
- App Service: `prod-bwf-usw2-app`
- Key Vault: `prod-bwf-usw2-kv`
- Managed Identity: `prod-bwf-usw2-id`

### Shared Resources
- Storage: `prodbwfst` (with cloud-specific endpoint)
- Cosmos DB: `prod-bwf-cosno` (with cloud-specific endpoint)
- Service Bus: `prod-bwf-sbns` (with cloud-specific endpoint)

## Environment Variable Syntax

### PowerShell
```powershell
$env:EnvironmentOptions__Cloud = "AzureCloud"
$env:EnvironmentOptions__Region = "westus2"
$env:EnvironmentOptions__RegionShortName = "usw2"
```

### Bash
```bash
export EnvironmentOptions__Cloud=AzureCloud
export EnvironmentOptions__Region=westus2
export EnvironmentOptions__RegionShortName=usw2
```

### Docker
```yaml
environment:
  - EnvironmentOptions__Cloud=AzureCloud
  - EnvironmentOptions__Region=westus2
  - EnvironmentOptions__RegionShortName=usw2
```

### Azure App Service (Portal)
```
Name: EnvironmentOptions__Cloud
Value: AzureCloud

Name: EnvironmentOptions__Region
Value: westus2

Name: EnvironmentOptions__RegionShortName
Value: usw2
```

## Checklist for New Deployment

- [ ] Set `ASPNETCORE_ENVIRONMENT` to match environment (Test, Ppe, Production)
- [ ] Set `EnvironmentOptions__Cloud` for target Azure cloud
- [ ] Set `EnvironmentOptions__Region` for target Azure region
- [ ] Set `EnvironmentOptions__RegionShortName` (abbreviated region)
- [ ] Verify correct `appsettings.{Environment}.json` exists
- [ ] Test that endpoints match expected cloud (check storage URL, etc.)
- [ ] Verify resource naming matches convention
- [ ] Check that shared resources are accessible from both regions

## Troubleshooting

**Problem:** Wrong cloud endpoints generated
- ? **Solution:** Check `EnvironmentOptions__Cloud` environment variable

**Problem:** Resources not found
- ? **Solution:** Verify Region and RegionShortName match actual Azure region

**Problem:** Shared resources have wrong URLs
- ? **Solution:** Ensure Cloud setting matches deployment cloud

**Problem:** Configuration not loading
- ? **Solution:** Check environment variable syntax (double underscore `__`)

## Related Documentation

- [EnvironmentOptions.md](EnvironmentOptions.md) - Complete configuration guide
- [EnvironmentConfiguration-Examples.md](EnvironmentConfiguration-Examples.md) - Deployment examples
- `Acl/src/EnvironmentOptions.cs` - Class definition
- `Acl/src/EnvironmentOptionsExts/` - Cloud-aware extension methods
