# FunctionalDddAspTemplate - Reference Implementation

> **This document describes the canonical patterns found in the FunctionalDddAspTemplate project.**
> 
> All new projects generated should follow these exact patterns.

---

## ?? **Project Structure**

```
BestWeatherForecast/  (Project name prefix for all namespaces)
??? Domain/
?   ??? src/
?   ?   ??? Aggregates/          # User.cs
?   ?   ??? ValueObjects/        # UserId.cs, FirstName.cs, LastName.cs
?   ?   ??? ZipCode.cs           # ScalarValueObject example
?   ?   ??? WeatherForecast.cs   # Domain model
?   ?   ??? DailyTemperature.cs  # Record type
?   ??? tests/
??? Application/
?   ??? src/
?   ?   ??? WeatherForcast/      # Feature folder
?   ?   ?   ??? WeatherForecastQuery.cs
?   ?   ?   ??? WeatherForecastQueryHandler.cs
?   ?   ??? Abstractions/        # IWeatherForecastService.cs
?   ?   ??? DependencyInjection.cs
?   ??? tests/
??? Acl/
?   ??? src/
?   ?   ??? WeatherForecastService.cs
?   ?   ??? DependencyInjection.cs
?   ??? tests/
??? Api/
?   ??? src/
?   ?   ??? 2023-06-06/          # Date-based versioning
?   ?   ?   ??? Controllers/
?   ?   ?   ?   ??? WeatherForecastController.cs
?   ?   ?   ?   ??? UsersController.cs
?   ?   ?   ??? Models/
?   ?   ?       ??? ConfigureMapster.cs
?   ?   ?       ??? WeatherForecast.cs (DTO)
?   ?   ?       ??? RegisterUserRequest.cs
?   ?   ??? Middleware/
?   ?   ?   ??? ErrorHandlingMiddleware.cs
?   ?   ??? Swagger/
?   ?   ?   ??? AddApiVersionMetadata.cs
?   ?   ?   ??? AddTraceParentParameter.cs
?   ?   ??? ConfigureSwaggerOptions.cs
?   ?   ??? ApiMeters.cs
?   ?   ??? DependencyInjection.cs
?   ?   ??? Program.cs
?   ??? tests/
```

---

## ??? **Domain Layer Patterns**

### **Aggregates (Uses AbstractValidator)**

```csharp
namespace BestWeatherForecast.Domain;

using FluentValidation;

public class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    public string Password { get; }

    public static Result<User> TryCreate(FirstName firstName, LastName lastName, EmailAddress email, string password)
    {
        var user = new User(firstName, lastName, email, password);
        var validator = new UserValidator();
        return validator.ValidateToResult(user);
    }

    private User(FirstName firstName, LastName lastName, EmailAddress email, string password)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
    }

    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(user => user.FirstName).NotNull();
            RuleFor(user => user.LastName).NotNull();
            RuleFor(user => user.Email).NotNull();
            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password must not be empty.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        }
    }
}
```

**Key Points:**
- ? Use `AbstractValidator<T>` as **nested class** in aggregates
- ? Create separate instance of validator in `TryCreate()`
- ? Call `validator.ValidateToResult(order)` not `s_validator.ValidateToResult()`

### **Value Objects - RequiredGuid (Source Generated)**

```csharp
namespace BestWeatherForecast.Domain;

public partial class UserId : RequiredGuid
{
}
```

**Pattern:**
- ? `partial class` - Required for source generator
- ? No body needed - generator creates all methods
- ? Inherits from `RequiredGuid`

### **Value Objects - RequiredString (Source Generated)**

```csharp
namespace BestWeatherForecast.Domain;

public partial class FirstName : RequiredString { }
public partial class LastName : RequiredString { }
```

**Pattern:**
- ? `partial class` - Required for source generator
- ? Empty body
- ? Inherits from `RequiredString`

### **Value Objects - ScalarValueObject (Custom with InlineValidator)**

```csharp
namespace BestWeatherForecast.Domain;

using FluentValidation;

public class ZipCode : ScalarValueObject<string>
{
    private ZipCode(string value) : base(value)
    {
    }

    public static Result<ZipCode> TryCreate(string value)
    {
        var zipCode = new ZipCode(value);
        return s_validation.ValidateToResult(zipCode);
    }

    static readonly InlineValidator<ZipCode> s_validation = new()
    {
        v => v.RuleFor(x => x.Value)
        .NotEmpty().OverridePropertyName("zipCode")
        .Matches(@"^\d{5}(?:[-\s]\d{4})?$").OverridePropertyName("zipCode") //US Zip codes.
    };
}
```

**Key Points:**
- ? Use `InlineValidator` for value objects
- ? Use `.OverridePropertyName()` to customize error field name
- ? Instantiate the value object first, then validate

---

## ?? **Application Layer Patterns**

### **Queries (Uses InlineValidator)**

```csharp
namespace BestWeatherForecast.Application.WeatherForcast;

using BestWeatherForecast.Domain;
using FluentValidation;
using Mediator;

public class WeatherForecastQuery : IQuery<Result<WeatherForecast>>
{
    public ZipCode ZipCode { get; }

    public static Result<WeatherForecastQuery> TryCreate(ZipCode zipCode)
        => s_validator.ValidateToResult(new WeatherForecastQuery(zipCode));

    private WeatherForecastQuery(ZipCode zipCode) => ZipCode = zipCode;
    
    private static readonly InlineValidator<WeatherForecastQuery> s_validator = new()
    {
        v => v.RuleFor(x => x.ZipCode).NotNull(),
    };
}
```

**Pattern:**
- ? Use `InlineValidator` (NOT AbstractValidator)
- ? Static readonly field
- ? Validate inline in `TryCreate()`

### **Feature Folder Organization**

```
Application/src/
??? WeatherForcast/                    # Feature name
    ??? WeatherForecastQuery.cs
    ??? WeatherForecastQueryHandler.cs
```

**Pattern:**
- ? Group query/command with handler in feature folder
- ? One folder per feature

---

## ?? **API Layer Patterns**

### **Controller (Railway-Oriented Programming)**

```csharp
namespace BestWeatherForecast.Api._2023_06_06.Controllers;

using Asp.Versioning;
using BestWeatherForecast.Application.WeatherForcast;
using BestWeatherForecast.Domain;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("2023-06-06")]
[Consumes("application/json")]
[Produces("application/json")]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ISender _sender;

    public WeatherForecastController(ISender sender) => _sender = sender;

    [HttpGet("{zipCode}")]
    [ProducesResponseType(typeof(Models.WeatherForecast), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<Models.WeatherForecast>> Get(
        string zipCode, 
        CancellationToken cancellationToken)
        => await ZipCode.TryCreate(zipCode)
            .Bind(static zipCode => WeatherForecastQuery.TryCreate(zipCode))
            .BindAsync(q => _sender.Send(q, cancellationToken))
            .MapAsync(r => r.Adapt<Models.WeatherForecast>())
            .ToActionResultAsync(this);
}
```

**Key Patterns:**
- ? Use `ValueTask<ActionResult<T>>` for return type
- ? Use `=> await` for method expression
- ? Chain with `Bind`, `BindAsync`, `MapAsync`, `ToActionResultAsync`
- ? Use `static` keyword in lambdas when possible
- ? Use `CancellationToken` parameter name (not `ct`)

### **API Versioning**

**Folder:** `2023-06-06` (hyphens)  
**Namespace:** `_2023_06_06` (underscores)  
**Attribute:** `[ApiVersion("2023-06-06")]`  
**Route:** `[Route("api/[controller]")]` (no version)

### **Mapster Configuration**

```csharp
namespace BestWeatherForecast.Api._2023_06_06.Models;

using Mapster;

public class ConfigureMapster
{
    public static void Config()
    {
        TypeAdapterConfig<Domain.WeatherForecast, WeatherForecast>.NewConfig()
            .Map(dest => dest.ZipCode, src => src.ZipCode.Value);
    }
}
```

**Pattern:**
- ? Static `Config()` method
- ? Called from `DependencyInjection.cs`
- ? Located in `Models` folder for each API version

---

## ?? **Configuration Files**

### **Directory.Build.props**

```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Sdk Name="DotNet.ReproducibleBuilds.Isolated" Version="1.1.1" />
  
  <PropertyGroup Label="General">
    <Authors>Xavier John</Authors>
    <Company>$(Authors)</Company>
    <IsTestProject>$(MSBuildProjectName.EndsWith('.Tests'))</IsTestProject>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>Latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="$(MSBuildProjectName).Tests" />
    <Using Include="FunctionalDdd" />
  </ItemGroup>

  <PropertyGroup Condition=" '$(IsTestProject)' == 'false' ">
    <RootNamespace>BestWeatherForecast.$(MSBuildProjectName.Replace(" ", "_"))</RootNamespace>
    <AssemblyName>BestWeatherForecast.$(MSBuildProjectName)</AssemblyName>
  </PropertyGroup>

  <ImportGroup Condition=" '$(IsTestProject)' == 'true' ">
    <Import Project="$(MSBuildThisFileDirectory)build/test.props"/>
  </ImportGroup>
</Project>
```

**Key Points:**
- ? Global `<Using Include="FunctionalDdd" />` - No need to import in every file
- ? Namespace pattern: `ProjectName.$(MSBuildProjectName)`
- ? Assembly pattern: `ProjectName.$(MSBuildProjectName)`

### **Directory.Packages.props**

```xml
<Project>
  <PropertyGroup>
    <FunctionalDddVersion>3.0.0-alpha.3</FunctionalDddVersion>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageVersion Include="FunctionalDdd.Asp" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.CommonValueObjectGenerator" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.CommonValueObjects" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.DomainDrivenDesign" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.FluentValidation" Version="$(FunctionalDddVersion)" />
    <PackageVersion Include="FunctionalDdd.RailwayOrientedProgramming" Version="$(FunctionalDddVersion)" />
    
    <!-- Other packages alphabetically -->
    <PackageVersion Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.0" />
    <PackageVersion Include="FluentValidation" Version="11.11.0" />
    <PackageVersion Include="Mapster" Version="7.4.0" />
    <PackageVersion Include="Mediator.Abstractions" Version="3.0.1" />
    <PackageVersion Include="Mediator.SourceGenerator" Version="3.0.1" />
    
    <!-- Test packages -->
    <PackageVersion Include="xunit.v3" Version="3.2.1" />
    <PackageVersion Include="FluentAssertions" Version="7.2.0" />
  </ItemGroup>
</Project>
```

---

## ? **Validation Pattern Summary**

| Type | Validator | Pattern | Example |
|------|-----------|---------|---------|
| **Aggregate** | `AbstractValidator<T>` | Nested class, instance created | `User`, `Order` |
| **Value Object** | `InlineValidator<T>` | Static field, validate on create | `ZipCode` |
| **Query** | `InlineValidator<T>` | Static field, validate in TryCreate | `WeatherForecastQuery` |
| **Command** | `InlineValidator<T>` | Static field, validate in TryCreate | `CreateOrderCommand` |

---

## ?? **Critical Differences from Current Instructions**

### **1. Aggregate Validation**

**Current (Wrong):**
```csharp
private static readonly InlineValidator<Order> s_validator = new()
{
    v => v.RuleFor(x => x.CustomerId).NotNull()
};
```

**Template (Correct):**
```csharp
public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(x => x.CustomerId).NotNull();
    }
}

// In TryCreate:
var validator = new OrderValidator();
return validator.ValidateToResult(order);
```

### **2. ScalarValueObject Validation**

**Template Pattern:**
```csharp
public static Result<ZipCode> TryCreate(string value)
{
    var zipCode = new ZipCode(value);  // Create first
    return s_validation.ValidateToResult(zipCode);  // Then validate
}
```

### **3. Namespace Pattern**

**Always:** `ProjectName.$(MSBuildProjectName)`
- Domain: `BestWeatherForecast.Domain`
- Application: `BestWeatherForecast.Application`
- Api: `BestWeatherForecast.Api`

---

## ?? **Reference Links**

- **FunctionalDDD Library**: https://github.com/xavierjohn/FunctionalDDD
- **Template Repository**: https://github.com/xavierjohn/FunctionalDddAspTemplate
