# FunctionalDDD Clean Architecture Agent

> **Specialized agent for scaffolding .NET 10 projects with Clean Architecture, FunctionalDDD, and Railway-Oriented Programming**

---

## 🚨 CRITICAL RULES - READ FIRST

### **Repository Setup (MANDATORY - FIRST STEP)**

**Before scaffolding any code, MUST create .gitignore file!**

If `.gitignore` is missing or empty, create one using the **official Visual Studio template** from:
https://github.com/github/gitignore/blob/main/VisualStudio.gitignore

**Must cover:**
- `bin/` and `obj/` directories
- `.vs/` folder
- `*.dll`, `*.exe`, `*.pdb`
- NuGet packages
- Test results
- User-specific files (`*.user`, `*.suo`)

**See detailed setup**: [Repository Setup](instructions/repository-setup.md)

---

### **Railway-Oriented Programming (MANDATORY)**

**NEVER check `IsFailed`, `IsFailure`, or `IsSuccess` in controller methods!**

✅ **ALWAYS use method chaining:**
```csharp
=> await ValueObject.TryCreate(input)
    .Bind(value => Command.TryCreate(value))
    .BindAsync(cmd => _sender.Send(cmd, ct))
    .MapAsync(result => result.Adapt<Dto>())
    .ToActionResultAsync(this);
```

**See detailed patterns**: [Railway-Oriented Programming](instructions/railway-patterns.md)

---

### **API Versioning (MANDATORY)**

- **Namespace**: `_2025_01_15` (underscores)
- **Folder**: `2025-01-15` (hyphens)
- **Attribute**: `[ApiVersion("2025-01-15")]` (current date, YYYY-MM-DD)
- **Route**: `[Route("api/[controller]")]` (NO version in route)

**See controller template**: [API Controllers](instructions/controllers.md)

---

### **EnvironmentOptions Configuration (MANDATORY)**

**Three-Tier Configuration Strategy:**

1. **`appsettings.json`** - Service-level settings (ServiceName only)
2. **`appsettings.{Environment}.json`** - Environment-specific (Environment: local, test, ppe, prod)
3. **Environment Variables** - Deployment-specific (Cloud, Region, RegionShortName)

**Example Configuration:**
```json
// appsettings.json
{
  "EnvironmentOptions": {
    "ServiceName": "BWF"
  }
}

// appsettings.Development.json
{
  "EnvironmentOptions": {
    "Environment": "local",
    "Region": "local",
    "RegionShortName": "local",
    "Cloud": "AzureCloud"
  }
}

// Environment Variables (deployment time)
EnvironmentOptions__Cloud=AzureCloud
EnvironmentOptions__Region=westus2
EnvironmentOptions__RegionShortName=usw2
```

**Why this matters:**
- ✅ Multi-cloud support (Commercial, Government, China, Germany)
- ✅ Multi-region BCDR deployments
- ✅ Same Docker image across all environments
- ✅ Convention-based Azure resource naming

**See detailed guide**: [EnvironmentOptions Configuration](../docs/EnvironmentOptions.md)

---

### **Required Files (MANDATORY)**

**MUST create these files:**
1. `Api/src/Properties/launchSettings.json` - VS Code/Visual Studio debugging with EnvironmentOptions
2. `Api/src/api.http` - API testing
3. `Api/src/appsettings.json` - Base configuration (ServiceName)
4. `Api/src/appsettings.Development.json` - Development environment settings
5. `Api/src/appsettings.Test.json` - Test environment settings
6. `Api/src/appsettings.Ppe.json` - Pre-production environment settings
7. `Api/src/appsettings.Production.json` - Production environment settings

**See file contents**: [Configuration Files](instructions/configuration-files.md)

---

## ✅ Pre-Generation Checklist

Before generating code, verify:

- [ ] **Created .gitignore file** (Visual Studio template) - DO THIS FIRST!
- [ ] Controllers use `=> await` method chaining (NO `if` checks)
- [ ] API version is current date: `[ApiVersion("2025-01-15")]`
- [ ] Namespace uses underscores: `_2025_01_15`
- [ ] Folder uses hyphens: `2025-01-15`
- [ ] Route is simple: `[Route("api/[controller]")]`
- [ ] Created `launchSettings.json` with EnvironmentOptions environment variables
- [ ] Created `api.http` in `Api/src/`
- [ ] Created all `appsettings.{Environment}.json` files
- [ ] `EnvironmentOptions` loaded in `Program.cs` and passed to DI methods
- [ ] Value Objects use `RequiredGuid` or `RequiredString` when possible
- [ ] Commands/Queries use `InlineValidator`
- [ ] **Tests created for Domain, Application, and API layers - WITH ACTUAL TEST CODE, NOT EMPTY!**
- [ ] **Mock services created in `Application.Tests/Mocks/` for all Application abstractions**
- [ ] **Every aggregate has test class with [Fact] methods**
- [ ] **Every handler has test class with success/failure tests**

---

## 📚 Detailed Documentation

### **Setup & Configuration**
- [Repository Setup](instructions/repository-setup.md) - .gitignore, cleaning up tracked binaries
- [Configuration Files](instructions/configuration-files.md) - Required files and settings
- [Infrastructure Setup](instructions/infrastructure-setup.md) - **DependencyInjection, Program.cs, API versioning, OpenTelemetry, SLI**
- [EnvironmentOptions Guide](../docs/EnvironmentOptions.md) - **Multi-cloud, multi-region configuration**
- [Environment Configuration Examples](../docs/EnvironmentConfiguration-Examples.md) - **Deployment examples**
- [EnvironmentOptions Quick Reference](../docs/EnvironmentOptions-QuickReference.md) - **Quick lookup**
- [FunctionalDDD API Reference](instructions/functionalddd-api-reference.md) - **Library API usage guide**

### **Architecture Patterns**
- [Railway-Oriented Programming](instructions/railway-patterns.md) - Method chaining, operators, best practices
- [API Controllers](instructions/controllers.md) - Controller templates, versioning, DTOs
- [Domain Models](instructions/domain-models.md) - Aggregates, value objects, domain events
- [CQRS Patterns](instructions/cqrs-patterns.md) - Commands, queries, handlers
- [Testing](instructions/testing-patterns.md) - Unit tests, mocks, test patterns

### **Reference Implementation**
- [Template Reference](instructions/template-reference.md) - **Canonical patterns from FunctionalDddAspTemplate**

---

## 🏗️ Project Structure

```
ProjectName/
├── Domain/
│   ├── src/              # Aggregates, ValueObjects, Events
│   └── tests/            # Domain unit tests
├── Application/
│   ├── src/              # Commands, Queries, Abstractions
│   └── tests/            # Handler tests + Mock services
├── Acl/
│   ├── src/
│   │   ├── EnvironmentOptions.cs
│   │   ├── EnvironmentOptionsExts/  # Cloud-aware resource naming
│   │   ├── Repositories/
│   │   └── Services/
│   └── tests/            # Integration tests
├── Api/
│   ├── src/
│   │   ├── 2025-01-15/   # Versioned controllers & models
│   │   ├── Properties/launchSettings.json
│   │   ├── api.http
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.Test.json
│   │   ├── appsettings.Ppe.json
│   │   ├── appsettings.Production.json
│   │   └── Program.cs
│   └── tests/            # API integration tests
├── docs/
│   ├── EnvironmentOptions.md
│   ├── EnvironmentConfiguration-Examples.md
│   └── EnvironmentOptions-QuickReference.md
├── build/
│   └── test.props
├── Directory.Build.props
├── Directory.Packages.props
└── ProjectName.sln
```

---

## 🎯 Technology Stack

- **.NET 10** with C# 14
- **FunctionalDDD** 3.0.0-alpha.3 packages (**MUST USE - latest prerelease**)
  - Official Repository: https://github.com/xavierjohn/FunctionalDDD
  - `FunctionalDdd.RailwayOrientedProgramming` - Result monad, railway operators
  - `FunctionalDdd.DomainDrivenDesign` - Provides Aggregate base class
  - `FunctionalDdd.CommonValueObjectGenerator` - Provides RequiredGuid, RequiredString
  - `FunctionalDdd.FluentValidation` - Provides InlineValidator
  - `FunctionalDdd.CommonValueObjects` - Provides EmailAddress, etc.
  - `FunctionalDdd.Asp` - Provides ToActionResultAsync
- **Mediator** for CQRS
- **FluentValidation** for validation
- **Mapster** for object mapping
- **Asp.Versioning** for API versioning
- **OpenTelemetry** for observability
- **ServiceLevelIndicators** for SLI/SLO tracking
- **xUnit v3** for testing (NOT v2!)
- **FluentAssertions** for test assertions

**❌ DO NOT use:**
- Custom Aggregate/RequiredGuid/RequiredString implementations
- Custom Result/Error types
- xUnit v2
- Hardcoded cloud types or regions in configuration files

**📚 For FunctionalDDD API documentation and examples:**
- Repository: https://github.com/xavierjohn/FunctionalDDD
- API Reference: [FunctionalDDD API Reference](instructions/functionalddd-api-reference.md)

---

## 🚀 Quick Start Templates

### **Program.cs with EnvironmentOptions**
```csharp
using ProjectName.AntiCorruptionLayer;
using ProjectName.Api;
using ProjectName.Application;

var builder = WebApplication.CreateBuilder(args);

// Load EnvironmentOptions early for use during setup
Program.EnvironmentOptions = builder.Configuration
    .GetSection(nameof(EnvironmentOptions))
    .Get<EnvironmentOptions>() ?? new EnvironmentOptions();

// Add services to the container
builder.Services
    .AddPresentation(Program.EnvironmentOptions)
    .AddApplication()
    .AddAntiCorruptionLayer(Program.EnvironmentOptions);

var app = builder.Build();

// Configure middleware...
app.Run();

public partial class Program
{
    internal static EnvironmentOptions EnvironmentOptions { get; set; } = new();
}
```

### **DependencyInjection with EnvironmentOptions**
```csharp
// Api/src/DependencyInjection.cs
public static IServiceCollection AddPresentation(
    this IServiceCollection services, 
    EnvironmentOptions environmentOptions)
{
    services.ConfigureOpenTelemetry(environmentOptions);
    services.ConfigureServiceLevelIndicators(environmentOptions);
    // ... other services
    return services;
}

private static IServiceCollection ConfigureOpenTelemetry(
    this IServiceCollection services, 
    EnvironmentOptions environmentOptions)
{
    void configureResource(ResourceBuilder r) => r.AddService(
        serviceName: environmentOptions.ServiceName,
        serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");
    
    services.AddOpenTelemetry()
        .ConfigureResource(configureResource)
        // ... configure metrics and tracing
    return services;
}
```

### **ACL DependencyInjection**
```csharp
// Acl/src/DependencyInjection.cs
public static IServiceCollection AddAntiCorruptionLayer(
    this IServiceCollection services, 
    EnvironmentOptions environmentOptions)
{
    services.AddSingleton(Options.Create(environmentOptions));
    // ... register repositories and services
    return services;
}
```

### **Using EnvironmentOptions in Services**
```csharp
public class StorageService
{
    private readonly EnvironmentOptions _options;

    public StorageService(IOptions<EnvironmentOptions> options)
    {
        _options = options.Value;
    }

    public string GetStorageUrl()
    {
        // Extension method generates cloud-aware URL
        return _options.GetBlobStorageSharedUrl();
        // Returns:
        // - AzureCloud: https://prodappst.blob.core.windows.net
        // - AzureUSGovernment: https://prodappst.blob.core.usgovcloudapi.net
        // - AzureChinaCloud: https://prodappst.blob.core.chinacloud.cn
    }
}
```

### **Controller Method (Railway-Oriented)**
```csharp
[HttpPost]
public async ValueTask<ActionResult<OrderDto>> Create(
    [FromBody] CreateOrderRequest request, 
    CancellationToken ct)
    => await CustomerId.TryCreate(request.CustomerId)
        .Bind(customerId => CreateOrderCommand.TryCreate(customerId))
        .BindAsync(command => _sender.Send(command, ct))
        .MapAsync(order => order.Adapt<OrderDto>())
        .ToActionResultAsync(this);
```

### **Value Object**
```csharp
public partial class OrderId : RequiredGuid { }
```

### **Domain Aggregate**
```csharp
public class Order : Aggregate<OrderId>
{
    public static Result<Order> TryCreate(CustomerId customerId)
    {
        var order = new Order(customerId);
        return s_validator.ValidateToResult(order);
    }
    
    private static readonly InlineValidator<Order> s_validator = new()
    {
        v => v.RuleFor(x => x.CustomerId).NotNull()
    };
}
```

### **Command + Handler**
```csharp
public class CreateOrderCommand : ICommand<Result<Order>>
{
    public static Result<CreateOrderCommand> TryCreate(CustomerId customerId)
        => s_validator.ValidateToResult(new CreateOrderCommand(customerId));
    
    private static readonly InlineValidator<CreateOrderCommand> s_validator = new()
    {
        v => v.RuleFor(x => x.CustomerId).NotNull()
    };
}

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<Order>>
{
    public async ValueTask<Result<Order>> Handle(CreateOrderCommand command, CancellationToken ct)
        => await Order.TryCreate(command.CustomerId)
            .TapAsync(order => _repository.AddAsync(order, ct))
            .TapAsync(_ => _repository.SaveChangesAsync(ct));
}
```

---

## 🌍 Multi-Cloud & Multi-Region Support

### **Supported Azure Clouds**
- **AzureCloud** - Commercial/Public cloud
- **AzureUSGovernment** - US Government cloud
- **AzureChinaCloud** - China cloud (21Vianet)
- **AzureGermanCloud** - Germany cloud

### **Resource Naming Convention**
- **Regional**: `{environment}-{servicename}-{regionshortname}-{resourcetype}`
  - Example: `prod-bwf-usw2-app` (App Service in West US 2)
- **Shared**: `{environment}-{servicename}-{resourcetype}`
  - Example: `prodbwfst` (Storage Account, cloud-specific endpoints)

### **Deployment Configuration**
```bash
# Commercial Cloud - West US 2
EnvironmentOptions__Cloud=AzureCloud
EnvironmentOptions__Region=westus2
EnvironmentOptions__RegionShortName=usw2

# Government Cloud - Virginia
EnvironmentOptions__Cloud=AzureUSGovernment
EnvironmentOptions__Region=usgovvirginia
EnvironmentOptions__RegionShortName=usgv

# China Cloud - East 2
EnvironmentOptions__Cloud=AzureChinaCloud
EnvironmentOptions__Region=chinaeast2
EnvironmentOptions__RegionShortName=cnea2
```

---

**For complete examples and anti-patterns, see the detailed documentation files above.**
