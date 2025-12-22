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

### **Required Files (MANDATORY)**

**MUST create these files:**
1. `Api/src/Properties/launchSettings.json` - VS Code/Visual Studio debugging
2. `Api/src/api.http` - API testing

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
- [ ] Created `launchSettings.json` in `Api/src/Properties/`
- [ ] Created `api.http` in `Api/src/`
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
│   ├── src/              # Repositories, External services
│   └── tests/            # Integration tests
├── Api/
│   ├── src/
│   │   ├── 2025-01-15/   # Versioned controllers & models
│   │   ├── Properties/launchSettings.json
│   │   ├── api.http
│   │   └── Program.cs
│   └── tests/            # API integration tests
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
- **xUnit v3** for testing (NOT v2!)
- **FluentAssertions** for test assertions

**❌ DO NOT use:**
- Custom Aggregate/RequiredGuid/RequiredString implementations
- Custom Result/Error types
- xUnit v2

**📚 For FunctionalDDD API documentation and examples:**
- Repository: https://github.com/xavierjohn/FunctionalDDD
- API Reference: [FunctionalDDD API Reference](instructions/functionalddd-api-reference.md)

---

## 🚀 Quick Start Templates

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

**For complete examples and anti-patterns, see the detailed documentation files above.**
