# 🏗️ FunctionalDDD Clean Architecture Template

**Production-ready .NET 10 template with AI-powered scaffolding via GitHub Copilot!**

[![Build](https://github.com/xavierjohn/FunctionalDddAspTemplate/actions/workflows/build.yml/badge.svg)](https://github.com/xavierjohn/FunctionalDddAspTemplate/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## ✨ What is This?

A comprehensive Clean Architecture template for building .NET Web APIs using **Domain-Driven Design (DDD)** and **Railway-Oriented Programming**. Start with a minimal, production-ready structure and grow it feature-by-feature using GitHub Copilot.

### 🚀 Two Ways to Use This Template

#### **📦 Quick Start: Use as GitHub Template**
Clone the complete structure and start coding:

```powershell
# Create from template
gh repo create my-project --template xavierjohn/FunctionalDddAspTemplate --public
cd my-project

# Start developing!
dotnet build
dotnet test
dotnet run --project Api/src
```

**You get**:
- ✅ 4-layer Clean Architecture
- ✅ Domain, Application, ACL, and API layers
- ✅ FunctionalDDD packages configured
- ✅ Tests in place
- ✅ CI/CD pipeline ready

#### **🤖 AI-Powered: Use with Copilot Agent**
Let GitHub Copilot scaffold features via issues:

```powershell
# Create feature via GitHub issue
gh issue create \
  --label "copilot-feature" \
  --title "Add Order aggregate with Submit behavior"
  
# Copilot generates:
# - Order.cs aggregate with validation
# - OrderId value object  
# - Submit() behavior with tests
# - Complete PR ready for review
```

**Result**: Each feature generated in ~2 minutes with tests and best practices! 🎯

📖 **[Learn more about AI agent features](.github/AGENT_README.md)**

---

## 🏗️ Architecture Overview

This template promotes a **layered architecture** approach following Clean Architecture and DDD principles:

### Domain Layer 🎯
The core of your system - pure business logic with no external dependencies.

- **Aggregates**: Cohesive units of business logic and data
- **Value Objects**: Immutable types enforcing business rules
- **Domain Events**: Track important state changes
- **Railway-Oriented Programming**: Explicit error handling with `Result<T>`

**Example**:
```csharp
public class Order : Aggregate<OrderId>
{
    public static Result<Order> TryCreate(CustomerId customerId)
    {
        var order = new Order(customerId);
        return s_validator.ValidateToResult(order);
    }
    
    public Result<Order> Submit()
        => this.ToResult()
            .Ensure(_ => Status == OrderStatus.Draft, 
                   Error.Validation("Can only submit draft orders"))
            .Tap(_ => Status = OrderStatus.Submitted);
}
```

### Application Layer 📋
Implements use cases using abstractions - CQRS pattern separates reads and writes.

- **Queries**: Read operations (e.g., `GetOrderByIdQuery`)
- **Commands**: Write operations (e.g., `CreateOrderCommand`)
- **Handlers**: Process queries/commands via Mediator
- **Abstractions**: Interfaces for external dependencies

**Example**:
```csharp
public class CreateOrderCommand : ICommand<Result<Order>>
{
    public CustomerId CustomerId { get; }
    
    public static Result<CreateOrderCommand> TryCreate(CustomerId customerId)
        => s_validator.ValidateToResult(new CreateOrderCommand(customerId));
}
```

### Anti-Corruption Layer (ACL) 🛡️
Shields the domain from external systems - implements application abstractions.

- **Repositories**: Data persistence implementations
- **External Services**: HTTP clients, message queues, etc.
- **Adapters**: Translate between domain and external models
- **Infrastructure**: Databases, file systems, third-party APIs

### API Layer 🌐
RESTful endpoints with automatic documentation and observability.

- **Controllers**: Railway-oriented request handling
- **API Versioning**: Date-based versioning (e.g., `2024-01-15/`)
- **Swagger/OpenAPI**: Auto-generated documentation
- **Middleware**: Error handling, logging, metrics
- **OpenTelemetry**: Distributed tracing and metrics

**Example**:
```csharp
[ApiController]
[ApiVersion("2024-01-15")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public async ValueTask<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
        => await CustomerId.TryCreate(request.CustomerId)
            .Bind(CreateOrderCommand.TryCreate)
            .BindAsync(cmd => _sender.Send(cmd, ct))
            .ToActionResultAsync(this);
}
```

---

## 🎯 Key Features

### ✅ Railway-Oriented Programming
Explicit error handling using the `Result<T>` monad - no exceptions for business logic.

```csharp
// All operations return Result<T>
Result<Order> orderResult = Order.TryCreate(customerId)
    .Bind(order => order.Submit())
    .Tap(order => _logger.LogInformation("Order submitted"));

if (orderResult.IsSuccess)
{
    var order = orderResult.Value;
}
else
{
    var error = orderResult.Error;
}
```

For details: [FunctionalDDD Library](https://github.com/xavierjohn/FunctionalDDD)

### ✅ CQRS Pattern
Commands modify state, queries read data - clear separation of concerns.

```csharp
// Command - changes state
public class CreateOrderCommand : ICommand<Result<Order>> { }

// Query - reads data
public class GetOrderByIdQuery : IQuery<Result<Order>> { }
```

### ✅ FluentValidation Integration
Validation built into aggregates, commands, and queries.

```csharp
private static readonly InlineValidator<Order> s_validator = new()
{
    v => v.RuleFor(x => x.CustomerId).NotNull(),
    v => v.RuleFor(x => x.OrderLines).NotEmpty()
};
```

### ✅ Service Level Indicators (SLI)
All API methods emit metrics with duration and status codes.

**View locally**:
- 📊 **Metrics**: [Prometheus](http://localhost:9090)
- 🔍 **Traces**: [Zipkin](http://localhost:9411/zipkin/)

Run observability stack:
```powershell
docker-compose up -d
```

For details: [ServiceLevelIndicators](https://github.com/xavierjohn/ServiceLevelIndicators)

### ✅ Central Package Management
All NuGet versions managed in `Directory.Packages.props` - no version conflicts.

```xml
<Project>
  <PropertyGroup>
    <FunctionalDddVersion>3.0.0</FunctionalDddVersion>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="FunctionalDdd.RailwayOrientedProgramming" Version="$(FunctionalDddVersion)" />
  </ItemGroup>
</Project>
```

### ✅ Convention Over Configuration
Resource names follow conventions using `EnvironmentOptions`:

```json
{
  "EnvironmentOptions": {
    "Environment": "test",
    "Region": "westus2",
    "RegionShortName": "usw2"
  }
}
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)
- [GitHub CLI](https://cli.github.com/) (optional, for AI features)
- IDE: Visual Studio 2022, VS Code, or JetBrains Rider

### Option 1: Use as Template (Recommended)

```powershell
# Create new repository from template
gh repo create my-awesome-api \
  --template xavierjohn/FunctionalDddAspTemplate \
  --public \
  --clone

cd my-awesome-api

# Build and run
dotnet build
dotnet test
dotnet run --project Api/src

# Navigate to Swagger
start https://localhost:5001/swagger
```

### Option 2: Manual Clone

```powershell
git clone https://github.com/xavierjohn/FunctionalDddAspTemplate.git my-project
cd my-project

# Remove git history and start fresh
Remove-Item -Recurse -Force .git
git init
git add .
git commit -m "Initial commit from template"

# Build and test
dotnet build
dotnet test
```

### Option 3: AI-Powered Scaffolding

Use GitHub Copilot to generate features:

```powershell
# Create from template
gh repo create my-project --template xavierjohn/FunctionalDddAspTemplate --public
cd my-project

# Add a feature via GitHub issue
gh issue create \
  --label "copilot-feature" \
  --title "Add Customer aggregate" \
  --body "Create Customer aggregate with email validation"

# Copilot creates PR with complete implementation!
```

**Learn more**: [AI Agent Guide](.github/AGENT_README.md)

---

## 📁 Project Structure

```
FunctionalDddAspTemplate/
├── Domain/                          # Core business logic
│   ├── src/
│   │   ├── Aggregates/             # Business entities
│   │   ├── ValueObjects/           # Immutable types
│   │   └── Domain.csproj
│   ├── tests/                      # Domain tests
│   └── dirs.proj
├── Application/                     # Use cases (CQRS)
│   ├── src/
│   │   ├── Queries/               # Read operations
│   │   ├── Commands/              # Write operations
│   │   ├── Abstractions/          # Interfaces
│   │   └── Application.csproj
│   ├── tests/
│   └── dirs.proj
├── Acl/                            # Anti-Corruption Layer
│   ├── src/
│   │   ├── Repositories/          # Data access
│   │   ├── Services/              # External integrations
│   │   └── AntiCorruptionLayer.csproj
│   ├── tests/
│   └── dirs.proj
├── Api/                            # REST API
│   ├── src/
│   │   ├── YYYY-MM-DD/            # Date-versioned controllers
│   │   ├── Middleware/
│   │   └── Api.csproj
│   ├── tests/
│   └── dirs.proj
├── .github/
│   ├── copilot-instructions.md    # AI agent configuration
│   ├── AGENT_README.md            # AI features guide
│   └── workflows/                 # CI/CD pipelines
├── docs/
│   └── agent/                     # AI agent documentation
├── examples/
│   └── specs/                     # Sample YAML specifications
├── Directory.Build.props           # Common build settings
├── Directory.Packages.props        # Central package versions
├── global.json                     # .NET SDK version
└── FunctionalDddAspTemplate.sln   # Solution file
```

---

## 🧪 Running Tests

```powershell
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests in specific project
dotnet test Domain/tests

# Watch mode (auto-run on changes)
dotnet watch test --project Domain/tests
```

---

## 🎓 Learning Resources

### **Video: Writing High Quality Services**

[![Watch the video](https://img.youtube.com/vi/6_1CAgskxfM/hqdefault.jpg)](https://www.youtube.com/embed/6_1CAgskxfM)

### **Sample Applications**

- **[BuberDinner](https://github.com/xavierjohn/BuberDinner)** - Complete meal booking platform
- **Domain Layer Examples**: See `Domain/src/Aggregates/` in this template

### **Documentation**

- **[FunctionalDDD Library](https://github.com/xavierjohn/FunctionalDDD)** - Railway-Oriented Programming
- **[Service Level Indicators](https://github.com/xavierjohn/ServiceLevelIndicators)** - Observability
- **[AI Agent Guide](.github/AGENT_README.md)** - GitHub Copilot features
- **[Feature Templates](.github/feature-template.md)** - How to request features

---

## 🤖 AI Agent Features

This template includes a **GitHub Copilot agent** that can scaffold features via issues:

### **Add Domain Aggregate**
```powershell
gh issue create \
  --label "copilot-feature" \
  --title "Add Product aggregate with inventory tracking"
```

### **Add CQRS Command**
```powershell
gh issue create \
  --label "copilot-feature" \
  --title "Add UpdateProductPriceCommand"
```

### **Add API Endpoint**
```powershell
gh issue create \
  --label "copilot-feature" \
  --title "Add PUT /api/products/{id}/price endpoint"
```

**Result**: Complete PR with implementation + tests in ~2 minutes! 🚀

📖 **[Full AI Agent Documentation](.github/AGENT_README.md)**

---

## 📦 NuGet Packages Included

This template comes pre-configured with:

### **FunctionalDDD Packages**
- `FunctionalDdd.RailwayOrientedProgramming` - Result monad, error handling
- `FunctionalDdd.DomainDrivenDesign` - Aggregates, value objects
- `FunctionalDdd.FluentValidation` - Validation integration
- `FunctionalDdd.CommonValueObjects` - Email, phone, etc.
- `FunctionalDdd.Asp` - ASP.NET Core extensions

### **CQRS & Validation**
- `Mediator` - Command/query handling
- `FluentValidation` - Validation rules

### **API**
- `Asp.Versioning.Mvc.ApiExplorer` - API versioning
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI
- `Microsoft.AspNetCore.OpenApi` - OpenAPI support

### **Observability**
- `OpenTelemetry.Extensions.Hosting` - Telemetry
- `OpenTelemetry.Instrumentation.AspNetCore` - ASP.NET tracing

### **Testing**
- `xunit.v3` - Testing framework
- `FluentAssertions` - Assertion library
- `coverlet.collector` - Code coverage

---

## 🛠️ Customization

### Rename the Project

1. **Rename solution file**:
   ```powershell
   Rename-Item "FunctionalDddAspTemplate.sln" "MyProject.sln"
   ```

2. **Update `Directory.Build.props`**:
   ```xml
   <PropertyGroup>
     <RootNamespace>MyCompany.MyProject.$(MSBuildProjectName)</RootNamespace>
     <AssemblyName>MyCompany.MyProject.$(MSBuildProjectName)</AssemblyName>
   </PropertyGroup>
   ```

3. **Rebuild**:
   ```powershell
   dotnet clean
   dotnet build
   ```

### Add a New Layer

```powershell
# Create new layer directory
New-Item -ItemType Directory -Path "MyLayer/src"
New-Item -ItemType Directory -Path "MyLayer/tests"

# Create project
dotnet new classlib -n MyLayer -o MyLayer/src
dotnet new xunit -n MyLayer.Tests -o MyLayer/tests

# Add to solution
dotnet sln add MyLayer/src/MyLayer.csproj
dotnet sln add MyLayer/tests/MyLayer.Tests.csproj
```

---

## 🤝 Contributing

Contributions welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built on [FunctionalDDD](https://github.com/xavierjohn/FunctionalDDD) by Xavier John
- Inspired by [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) by Uncle Bob
- Railway-Oriented Programming from [F# for Fun and Profit](https://fsharpforfunandprofit.com/rop/)
- Powered by GitHub Copilot

---

## 📞 Support

- 📖 [Documentation](https://github.com/xavierjohn/FunctionalDDD)
- 💬 [Discussions](https://github.com/xavierjohn/FunctionalDddAspTemplate/discussions)
- 🐛 [Issues](https://github.com/xavierjohn/FunctionalDddAspTemplate/issues)

---

## ⭐ Star This Repository

If you find this template helpful, please give it a star! It helps others discover it.

---

**Ready to build your next .NET API?** 🚀

```powershell
gh repo create my-api --template xavierjohn/FunctionalDddAspTemplate --public
cd my-api
dotnet run --project Api/src
```

*Production-ready .NET APIs with Clean Architecture - in minutes, not weeks.*
