# Required Configuration Files

## 🚨 CRITICAL: Use FunctionalDDD Library Packages

**DO NOT implement your own base classes!**

**Official Documentation**: https://github.com/xavierjohn/FunctionalDDD

**MUST use these FunctionalDDD packages (latest prerelease versions):**
- ✅ `FunctionalDdd.RailwayOrientedProgramming` - Result monad, railway operators
- ✅ `FunctionalDdd.DomainDrivenDesign` - Aggregate base class
- ✅ `FunctionalDdd.CommonValueObjects` - EmailAddress and other value objects
- ✅ `FunctionalDdd.CommonValueObjectGenerator` - RequiredGuid, RequiredString generators
- ✅ `FunctionalDdd.FluentValidation` - InlineValidator, validation extensions
- ✅ `FunctionalDdd.Asp` - ToActionResultAsync and ASP.NET extensions

**DO NOT use:**
- ❌ Custom Aggregate base class (use FunctionalDdd.DomainDrivenDesign)
- ❌ Custom RequiredGuid/RequiredString (use FunctionalDdd.CommonValueObjectGenerator)
- ❌ Custom Result/Error types (use FunctionalDdd.RailwayOrientedProgramming)

---

## 📄 launchSettings.json

**Location**: `Api/src/Properties/launchSettings.json`

**Purpose**: Enable debugging in VS Code and Visual Studio

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:5000",
      "sslPort": 5001
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 🌐 api.http

**Filename**: `api.http` (exactly - NOT `ProjectName.Api.http`)

**Location**: `Api/src/api.http`

**Purpose**: Test APIs directly in VS Code and Visual Studio

**IMPORTANT**: The file must be named exactly `api.http` in the `Api/src/` directory, not prefixed with the project name.

```http
@baseUrl = http://localhost:5000
@apiVersion = 2025-01-15

### Health Check
GET {{baseUrl}}/health

### Create Order
POST {{baseUrl}}/api/orders
Content-Type: application/json
Api-Version: {{apiVersion}}

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000"
}

### Get Order by ID
@orderId = 550e8400-e29b-41d4-a716-446655440000
GET {{baseUrl}}/api/orders/{{orderId}}
Api-Version: {{apiVersion}}

### Update Order
PUT {{baseUrl}}/api/orders/{{orderId}}
Content-Type: application/json
Api-Version: {{apiVersion}}

{
  "customerId": "550e8400-e29b-41d4-a716-446655440001"
}

### Delete Order
DELETE {{baseUrl}}/api/orders/{{orderId}}
Api-Version: {{apiVersion}}

### Get All Orders
GET {{baseUrl}}/api/orders
Api-Version: {{apiVersion}}
```

---

## 📦 Directory.Packages.props

**Location**: Root directory

**Purpose**: Central package version management

**CRITICAL: Use FunctionalDDD packages (latest prerelease), NOT FluentResults or custom implementations!**

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <FunctionalDddVersion>3.0.0-alpha.3</FunctionalDddVersion>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- FunctionalDDD packages - MANDATORY! -->
    <PackageVersion Include="FunctionalDdd.RailwayOrientedProgramming" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.DomainDrivenDesign" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.FluentValidation" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.CommonValueObjects" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.CommonValueObjectGenerator" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.Asp" Version="$(FunctionalDddVersion)" />
    
    <!-- CQRS and Validation -->
    <PackageVersion Include="Mediator.Abstractions" Version="3.0.1" />
    <PackageVersion Include="Mediator.SourceGenerator" Version="3.0.1" />
    <PackageVersion Include="FluentValidation" Version="11.11.0" />
    
    <!-- ASP.NET Core -->
    <PackageVersion Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.0" />
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.1" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="10.1.0" />
    
    <!-- Mapping -->
    <PackageVersion Include="Mapster" Version="7.4.0" />
    
    <!-- Testing - Use xUnit v3! -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageVersion Include="xunit.v3" Version="3.2.1" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.5" />
    <PackageVersion Include="FluentAssertions" Version="7.2.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.4" />
  </ItemGroup>
</Project>
```

**Project files should NOT specify versions:**
```xml
<ItemGroup>
  <PackageReference Include="FunctionalDdd.RailwayOrientedProgramming" />
</ItemGroup>
```

---

## 🏗️ Directory.Build.props

**Location**: Root directory

```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Sdk Name="DotNet.ReproducibleBuilds.Isolated" Version="1.1.1" />
  
  <PropertyGroup Label="General">
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>Latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <Using Include="FunctionalDdd" />
  </ItemGroup>
</Project>
```

---

## 🧪 build/test.props

**Location**: `build/test.props`

```xml
<Project>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" />
  </ItemGroup>
</Project>
