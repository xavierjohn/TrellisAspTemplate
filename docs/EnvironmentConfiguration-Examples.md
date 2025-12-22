# Environment Configuration Examples

This document shows how to configure `EnvironmentOptions` for different deployment scenarios including multi-region and multi-cloud deployments.

## Local Development

### Using launchSettings.json (Already Configured)
```json
{
  "profiles": {
    "https": {
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "EnvironmentOptions__Cloud": "AzureCloud",
        "EnvironmentOptions__Region": "westus2",
        "EnvironmentOptions__RegionShortName": "usw2"
      }
    }
  }
}
```

### Using .NET User Secrets (Alternative)
```bash
cd Api/src
dotnet user-secrets set "EnvironmentOptions:Cloud" "AzureCloud"
dotnet user-secrets set "EnvironmentOptions:Region" "westus2"
dotnet user-secrets set "EnvironmentOptions:RegionShortName" "usw2"
```

## Azure Commercial Cloud Deployments

### Azure App Service - Portal Configuration
1. Navigate to: Configuration ? Application settings
2. Add new application settings:
   - `EnvironmentOptions__Cloud` = `AzureCloud`
   - `EnvironmentOptions__Region` = `westus2`
   - `EnvironmentOptions__RegionShortName` = `usw2`

### Azure CLI - Multi-Region
```bash
# Test Environment - West US 2 (Primary)
az webapp config appsettings set \
  --name test-bwf-usw2-app \
  --resource-group test-bwf-rg \
  --settings \
    EnvironmentOptions__Cloud=AzureCloud \
    EnvironmentOptions__Region=westus2 \
    EnvironmentOptions__RegionShortName=usw2

# Test Environment - East US 2 (DR)
az webapp config appsettings set \
  --name test-bwf-use2-app \
  --resource-group test-bwf-rg \
  --settings \
    EnvironmentOptions__Cloud=AzureCloud \
    EnvironmentOptions__Region=eastus2 \
    EnvironmentOptions__RegionShortName=use2
```

### Bicep/ARM Template
```bicep
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: 'test-bwf-usw2-app'
  location: 'westus2'
  properties: {
    siteConfig: {
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Test'
        }
        {
          name: 'EnvironmentOptions__Cloud'
          value: 'AzureCloud'
        }
        {
          name: 'EnvironmentOptions__Region'
          value: 'westus2'
        }
        {
          name: 'EnvironmentOptions__RegionShortName'
          value: 'usw2'
        }
      ]
    }
  }
}
```

## Azure Government Cloud Deployments

### Azure CLI - Government Cloud
```bash
# Production - US Gov Virginia
az webapp config appsettings set \
  --name prod-bwf-usgv-app \
  --resource-group prod-bwf-rg \
  --settings \
    EnvironmentOptions__Cloud=AzureUSGovernment \
    EnvironmentOptions__Region=usgovvirginia \
    EnvironmentOptions__RegionShortName=usgv

# Production - US Gov Texas (DR)
az webapp config appsettings set \
  --name prod-bwf-usgt-app \
  --resource-group prod-bwf-rg \
  --settings \
    EnvironmentOptions__Cloud=AzureUSGovernment \
    EnvironmentOptions__Region=usgovtexas \
    EnvironmentOptions__RegionShortName=usgt
```

### Bicep Template - Government Cloud
```bicep
resource webAppGov 'Microsoft.Web/sites@2023-01-01' = {
  name: 'prod-bwf-usgv-app'
  location: 'usgovvirginia'
  properties: {
    siteConfig: {
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'EnvironmentOptions__Cloud'
          value: 'AzureUSGovernment'
        }
        {
          name: 'EnvironmentOptions__Region'
          value: 'usgovvirginia'
        }
        {
          name: 'EnvironmentOptions__RegionShortName'
          value: 'usgv'
        }
      ]
    }
  }
}
```

## Azure China Cloud Deployments

### Azure CLI - China Cloud
```bash
# Production - China East 2
az webapp config appsettings set \
  --name prod-bwf-cnea2-app \
  --resource-group prod-bwf-rg \
  --settings \
    EnvironmentOptions__Cloud=AzureChinaCloud \
    EnvironmentOptions__Region=chinaeast2 \
    EnvironmentOptions__RegionShortName=cnea2
```

## Azure Container Apps

### YAML Deployment - Commercial Cloud
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bwf-api
spec:
  template:
    spec:
      containers:
      - name: bwf-api
        image: myregistry.azurecr.io/bwf-api:latest
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Test"
        - name: EnvironmentOptions__Cloud
          value: "AzureCloud"
        - name: EnvironmentOptions__Region
          value: "westus2"
        - name: EnvironmentOptions__RegionShortName
          value: "usw2"
```

### Azure CLI - Government Cloud
```bash
az containerapp create \
  --name prod-bwf-usgv-app \
  --resource-group prod-bwf-rg \
  --environment prod-bwf-env \
  --image myregistry.azurecr.io/bwf-api:latest \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    EnvironmentOptions__Cloud=AzureUSGovernment \
    EnvironmentOptions__Region=usgovvirginia \
    EnvironmentOptions__RegionShortName=usgv
```

## Docker / Docker Compose

### docker-compose.yml - Commercial Cloud
```yaml
version: '3.8'
services:
  api:
    image: bwf-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Test
      - EnvironmentOptions__Cloud=AzureCloud
      - EnvironmentOptions__Region=westus2
      - EnvironmentOptions__RegionShortName=usw2
    ports:
      - "5122:8080"
```

### docker-compose.yml - Government Cloud
```yaml
version: '3.8'
services:
  api-gov:
    image: bwf-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - EnvironmentOptions__Cloud=AzureUSGovernment
      - EnvironmentOptions__Region=usgovvirginia
      - EnvironmentOptions__RegionShortName=usgv
    ports:
      - "5122:8080"
```

### Docker Run - China Cloud
```bash
docker run -d \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e EnvironmentOptions__Cloud=AzureChinaCloud \
  -e EnvironmentOptions__Region=chinaeast2 \
  -e EnvironmentOptions__RegionShortName=cnea2 \
  -p 5122:8080 \
  bwf-api:latest
```

## Kubernetes

### ConfigMap - Multi-Cloud Setup
```yaml
---
# Commercial Cloud - West US 2
apiVersion: v1
kind: ConfigMap
metadata:
  name: bwf-config-commercial-westus2
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  EnvironmentOptions__Cloud: "AzureCloud"
  EnvironmentOptions__Region: "westus2"
  EnvironmentOptions__RegionShortName: "usw2"
---
# Government Cloud - US Gov Virginia
apiVersion: v1
kind: ConfigMap
metadata:
  name: bwf-config-government-usgv
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  EnvironmentOptions__Cloud: "AzureUSGovernment"
  EnvironmentOptions__Region: "usgovvirginia"
  EnvironmentOptions__RegionShortName: "usgv"
---
# Deployment using Commercial Cloud config
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bwf-api-commercial
spec:
  replicas: 3
  template:
    spec:
      containers:
      - name: api
        image: myregistry.azurecr.io/bwf-api:latest
        envFrom:
        - configMapRef:
            name: bwf-config-commercial-westus2
```

### Helm Chart (values.yaml)
```yaml
# values-prod-commercial.yaml
environment: Production
cloud: AzureCloud
region:
  name: westus2
  shortName: usw2

---
# values-prod-government.yaml
environment: Production
cloud: AzureUSGovernment
region:
  name: usgovvirginia
  shortName: usgv

---
# Deployment template (templates/deployment.yaml)
env:
  - name: ASPNETCORE_ENVIRONMENT
    value: {{ .Values.environment }}
  - name: EnvironmentOptions__Cloud
    value: {{ .Values.cloud }}
  - name: EnvironmentOptions__Region
    value: {{ .Values.region.name }}
  - name: EnvironmentOptions__RegionShortName
    value: {{ .Values.region.shortName }}
```

Deploy to different clouds:
```bash
# Commercial Cloud
helm install bwf-api ./chart -f values-prod-commercial.yaml

# Government Cloud
helm install bwf-api-gov ./chart -f values-prod-government.yaml
```

## CI/CD Pipeline Examples

### GitHub Actions - Multi-Cloud Deployment
```yaml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  deploy-commercial-westus2:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Commercial Cloud - West US 2
        uses: azure/webapps-deploy@v2
        with:
          app-name: prod-bwf-usw2-app
          # Cloud and Region configured in Azure App Service

  deploy-commercial-eastus2:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Commercial Cloud - East US 2
        uses: azure/webapps-deploy@v2
        with:
          app-name: prod-bwf-use2-app

  deploy-government-virginia:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Government Cloud - Virginia
        uses: azure/webapps-deploy@v2
        with:
          app-name: prod-bwf-usgv-app
          # Government cloud endpoints configured automatically
```

### Azure DevOps Pipeline - Multi-Cloud
```yaml
stages:
# Commercial Cloud Deployments
- stage: DeployCommercialWestUS2
  jobs:
  - deployment: DeployAPI
    environment: prod-commercial-westus2
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: 'Azure-Commercial-Subscription'
              appName: 'prod-bwf-usw2-app'
              appSettings: |
                -EnvironmentOptions__Cloud "AzureCloud"
                -EnvironmentOptions__Region "westus2"
                -EnvironmentOptions__RegionShortName "usw2"

# Government Cloud Deployments
- stage: DeployGovernmentVirginia
  jobs:
  - deployment: DeployAPI
    environment: prod-government-usgv
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: 'Azure-Government-Subscription'
              appName: 'prod-bwf-usgv-app'
              appSettings: |
                -EnvironmentOptions__Cloud "AzureUSGovernment"
                -EnvironmentOptions__Region "usgovvirginia"
                -EnvironmentOptions__RegionShortName "usgv"
```

## Multi-Cloud Deployment Matrix

| Environment | Cloud | Primary Region | DR Region | Shared Resources |
|-------------|-------|---------------|-----------|------------------|
| **Development** | Commercial | local | - | N/A |
| **Test** | Commercial | westus2 | eastus2 | test-bwf-st (*.windows.net) |
| **PPE** | Commercial | westus2 | eastus2 | ppe-bwf-st (*.windows.net) |
| **Production** | Commercial | westus2 | eastus2 | prod-bwf-st (*.windows.net) |
| **Production** | Government | usgovvirginia | usgovtexas | prod-bwf-st (*.usgovcloudapi.net) |
| **Production** | China | chinaeast2 | chinanorth2 | prod-bwf-st (*.chinacloud.cn) |

## Validation Script

Use this PowerShell script to verify configuration:

```powershell
# Start-ApiWithConfig.ps1
param(
    [string]$Environment = "Development",
    [string]$Cloud = "AzureCloud",
    [string]$Region = "westus2",
    [string]$RegionShortName = "usw2"
)

$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:EnvironmentOptions__Cloud = $Cloud
$env:EnvironmentOptions__Region = $Region
$env:EnvironmentOptions__RegionShortName = $RegionShortName

Write-Host "Starting API with configuration:" -ForegroundColor Green
Write-Host "  Environment: $Environment"
Write-Host "  Cloud: $Cloud"
Write-Host "  Region: $Region ($RegionShortName)"

dotnet run --project Api/src
```

Usage examples:
```powershell
# Commercial Cloud - West US 2
.\Start-ApiWithConfig.ps1 -Environment Production -Cloud AzureCloud -Region westus2 -RegionShortName usw2

# Government Cloud - Virginia
.\Start-ApiWithConfig.ps1 -Environment Production -Cloud AzureUSGovernment -Region usgovvirginia -RegionShortName usgv

# China Cloud - East 2
.\Start-ApiWithConfig.ps1 -Environment Production -Cloud AzureChinaCloud -Region chinaeast2 -RegionShortName cnea2
```

## Testing Endpoint Generation

Verify that the correct cloud endpoints are generated:

```bash
# Set environment for Commercial Cloud
export ASPNETCORE_ENVIRONMENT=Test
export EnvironmentOptions__Cloud=AzureCloud
export EnvironmentOptions__Region=westus2
export EnvironmentOptions__RegionShortName=usw2

# Start the API and check Swagger/logs for generated endpoints
dotnet run --project Api/src

# Expected storage endpoint: https://testbwfst.blob.core.windows.net
```

```bash
# Set environment for Government Cloud
export ASPNETCORE_ENVIRONMENT=Test
export EnvironmentOptions__Cloud=AzureUSGovernment
export EnvironmentOptions__Region=usgovvirginia
export EnvironmentOptions__RegionShortName=usgv

# Start the API
dotnet run --project Api/src

# Expected storage endpoint: https://testbwfst.blob.core.usgovcloudapi.net
```

## Security Considerations

### Sovereign Cloud Isolation
- ? Government cloud deployments are automatically isolated
- ? Endpoints use sovereign cloud domains (.usgovcloudapi.net, .chinacloud.cn)
- ? Same code, different runtime configuration
- ? No code changes needed for compliance

### Best Practices
1. Use separate Azure subscriptions for different clouds
2. Separate deployment pipelines for sovereign clouds
3. Test cloud-specific endpoints in pre-production
4. Document compliance requirements per cloud
5. Use managed identities for authentication (works across all clouds)
