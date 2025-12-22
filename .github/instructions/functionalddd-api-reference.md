# FunctionalDDD Library API Reference

> **Official Documentation**: https://github.com/xavierjohn/FunctionalDDD
> 
> **NuGet Packages**: https://www.nuget.org/profiles/xavierjohn

## ?? Package Version

**FunctionalDDD 3.0.0-alpha.3** (latest prerelease)

All packages use the same version number.

**For detailed documentation, examples, and API reference, see the official repository:**
https://github.com/xavierjohn/FunctionalDDD

---

## ?? Quick Reference Guide

This is a condensed reference. For complete documentation, refer to the official repository.

---

## ?? RequiredGuid (ID Types)

### **Creating New IDs**

```csharp
// Generate a new unique ID
var orderId = OrderId.NewUnique();

// Create from existing Guid
var result = OrderId.TryCreate(existingGuid);

// Create from string
var result = OrderId.TryCreate("550e8400-e29b-41d4-a716-446655440000");
```

### **Definition**

```csharp
// Uses source generator - must be partial class
public partial class OrderId : RequiredGuid { }
public partial class CustomerId : RequiredGuid { }
public partial class TodoItemId : RequiredGuid { }
```

**Important**: 
- ? Must be `partial class`
- ? No attributes needed - source generator works automatically
- ? Inherited from `FunctionalDdd.CommonValueObjectGenerator`

---

## ?? RequiredString (String Types)

### **Creating Values**

```csharp
// Create from string
var result = Title.TryCreate("My Title");

// Returns Result<Title>
if (result.IsSuccess)
{
    var title = result.Value;
}
```

### **Definition**

```csharp
// Uses source generator - must be partial class
public partial class Title : RequiredString { }
public partial class FirstName : RequiredString { }
public partial class LastName : RequiredString { }
```

**Important**:
- ? Must be `partial class`
- ? No attributes needed
- ? Automatically validates non-null/non-empty
- ? Inherited from `FunctionalDdd.CommonValueObjectGenerator`

---

## ? Result<T> Type

### **Creating Success Results**

```csharp
// Success with value
return Result.Success(order);

// Success without value (returns Result<Unit>)
return Result.Success();
```

### **Creating Failure Results**

```csharp
// Failure with type parameter
return Result.Failure<Order>(Error.Validation("Invalid order"));

// Failure without type parameter (returns Result<Unit>)
return Result.Failure(Error.Validation("Operation failed"));

// Not found with instance
return Result.Failure<Order>(Error.NotFound("Order not found", instance: orderId));
```

### **Error Types**

```csharp
// Validation error - field-level validation failures
Error.Validation("Email format is invalid", "email");
Error.Validation("Password too short", "password");

// Bad request error - malformed requests
Error.BadRequest("Invalid JSON payload");

// Not found error - resource not found
Error.NotFound("Order not found");
Error.NotFound($"User {userId} not found", userId);

// Conflict error - state conflicts
Error.Conflict("Email address already in use");
Error.Conflict("Order already exists");

// Unauthorized error - authentication required
Error.Unauthorized("Login required to access this resource");

// Forbidden error - insufficient permissions
Error.Forbidden("Admin access required");

// Domain error - business rule violations
Error.Domain("Cannot withdraw more than account balance");

// Rate limit error - quota exceeded
Error.RateLimit("API rate limit exceeded. Retry in 60 seconds");

// Service unavailable error - temporary unavailability
Error.ServiceUnavailable("Service under maintenance");

// Unexpected error - system errors
Error.Unexpected("Database connection failed");
```

**Choosing the Right Error Type:**
- **ValidationError** - Field-level input validation (invalid email format, missing fields)
- **BadRequestError** - Malformed requests (invalid JSON, bad query parameters)
- **DomainError** - Business logic violations (insufficient funds, order limits)
- **ConflictError** - State-based conflicts (duplicate email, concurrent modification)
- **NotFoundError** - Resource doesn't exist
- **UnauthorizedError** - Not authenticated (not logged in)
- **ForbiddenError** - No permission (logged in but forbidden)
- **RateLimitError** - Too many requests
- **ServiceUnavailableError** - Temporary unavailability
- **UnexpectedError** - Infrastructure/system failures

### **Checking Results**

```csharp
// ? DON'T check in controllers - use railway chaining!
if (result.IsSuccess) { }
if (result.IsFailure) { }

// ? Use in domain/application layer for logic
public Result<Order> Submit()
{
    if (Status != OrderStatus.Draft)
        return Result.Failure<Order>(Error.Validation("Can only submit draft orders"));
    
    // Or use Ensure for functional style
    return this.ToResult()
        .Ensure(_ => Status == OrderStatus.Draft, 
               Error.Validation("Can only submit draft orders"));
}
```

---

## ?? Railway Operators

### **Bind / BindAsync**

Chain operations that return `Result<T>`:

```csharp
// Synchronous
Result<OrderId> idResult = OrderId.TryCreate(input)
    .Bind(id => ValidateId(id))
    .Bind(id => LoadOrder(id));

// Asynchronous
Result<Order> orderResult = await OrderId.TryCreate(input)
    .BindAsync(id => GetOrderByIdAsync(id, ct))
    .BindAsync(order => EnrichOrderAsync(order, ct));
```

### **Map / MapAsync**

Transform the success value:

```csharp
// Synchronous
Result<OrderDto> dto = orderResult
    .Map(order => order.Adapt<OrderDto>());

// Asynchronous  
Result<OrderDto> dto = await orderResult
    .MapAsync(order => ConvertToDtoAsync(order));
```

### **Tap / TapAsync**

Execute side effects without changing the result:

```csharp
// Synchronous
Result<Order> result = orderResult
    .Tap(order => _logger.LogInformation("Order created: {Id}", order.Id));

// Asynchronous
Result<Order> result = await orderResult
    .TapAsync(order => _repository.AddAsync(order, ct))
    .TapAsync(_ => _repository.SaveChangesAsync(ct));
```

### **Ensure**

Add validation inline:

```csharp
Result<Order> result = order.ToResult()
    .Ensure(o => o.Items.Any(), Error.Validation("Order must have items"))
    .Ensure(o => o.Total > 0, Error.Validation("Order total must be positive"));
```

### **Combine**

Merge multiple Results before binding:

```csharp
// Combine 2 results
Result<(OrderId, CustomerId)> combined = OrderId.TryCreate(orderId)
    .Combine(CustomerId.TryCreate(customerId));

// Use with Bind to destructure
Result<Order> order = OrderId.TryCreate(orderId)
    .Combine(CustomerId.TryCreate(customerId))
    .Bind((orderId, customerId) => UpdateOrderCommand.TryCreate(orderId, customerId));
```

### **Match**

Pattern match on success/failure:

```csharp
ActionResult result = OrderId.TryCreate(id).Match(
    ok => Ok(ok),
    err => BadRequest(err));

// With transformation
string message = orderResult.Match(
    order => $"Order {order.Id} created",
    errors => $"Failed: {string.Join(", ", errors.Select(e => e.Message))}");
```

### **ToActionResult / ToActionResultAsync**

Convert Result to HTTP response (in controllers):

```csharp
// Synchronous
public ActionResult<OrderDto> Get(string id) =>
    OrderId.TryCreate(id)
        .Bind(orderId => GetOrder(orderId))
        .Map(order => order.Adapt<OrderDto>())
        .ToActionResult(this);

// Asynchronous
public async ValueTask<ActionResult<OrderDto>> Get(string id, CancellationToken ct) =>
    await OrderId.TryCreate(id)
        .BindAsync(orderId => GetOrderAsync(orderId, ct))
        .MapAsync(order => order.Adapt<OrderDto>())
        .ToActionResultAsync(this);
```

---

## ?? Unit Type

Use `Unit` for operations that return no value:

```csharp
// In method signatures
public Result<Unit> Delete(OrderId id)

// Returning Unit - Three ways:
return Result.Success();                    // Simplest - returns Result<Unit>
return Result.Success<Unit>(Unit.Value);    // Explicit type
return Result.Success(Unit.Value);          // Value parameter

// Failure with Unit
return Result.Failure(Error.NotFound("Not found"));              // Returns Result<Unit>
return Result.Failure<Unit>(Error.NotFound("Not found"));        // Explicit type

// With railway chaining
public ActionResult<Unit> Delete(string id) =>
    OrderId.TryCreate(id).Match(
        ok => NoContent(),
        err => err.ToActionResult<Unit>(this));

// In handlers - simplest approach
public async ValueTask<Result<Unit>> Handle(DeleteOrderCommand command, CancellationToken ct)
    => await _repository.DeleteAsync(command.OrderId, ct)
        .TapAsync(_ => _repository.SaveChangesAsync(ct))
        .MapAsync(_ => Result.Success());  // Returns Result<Unit>
```

**Access Unit value:**
- `Unit.Value` - The singleton Unit instance

**Tip:** Use the parameterless `Result.Success()` and `Result.Failure(error)` for cleaner code when working with Unit results.

---

## ??? Aggregate<TId>

```csharp
using FunctionalDdd;

public class Order : Aggregate<OrderId>
{
    // Constructor must call base with ID
    private Order(CustomerId customerId) : base(OrderId.NewUnique())
    {
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        
        // Add domain events
        DomainEvents.Add(new OrderCreatedEvent(Id, customerId));
    }
    
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

**Key Points:**
- ? Inherits from `Aggregate<TId>` where TId is your ID type
- ? ID is set in constructor via `base(OrderId.NewUnique())`
- ? Access ID via `this.Id` property
- ? Domain events via `DomainEvents.Add(...)`

---

## ?? InlineValidator

```csharp
using FluentValidation;

public class CreateOrderCommand : ICommand<Result<Order>>
{
    public CustomerId CustomerId { get; }
    
    public static Result<CreateOrderCommand> TryCreate(CustomerId customerId)
        => s_validator.ValidateToResult(new CreateOrderCommand(customerId));
    
    private CreateOrderCommand(CustomerId customerId) => CustomerId = customerId;
    
    // InlineValidator from FunctionalDdd.FluentValidation
    private static readonly InlineValidator<CreateOrderCommand> s_validator = new()
    {
        v => v.RuleFor(x => x.CustomerId).NotNull()
    };
}
```

**Usage:**
- ? Static field with `InlineValidator<T>`
- ? Initialize with collection initializer syntax
- ? Use `ValidateToResult()` to get `Result<T>`

---

## ?? Common Value Objects (from library)

### **EmailAddress**

```csharp
using FunctionalDdd;

// Use directly - don't implement!
var result = EmailAddress.TryCreate("user@example.com");

if (result.IsSuccess)
{
    var email = result.Value;
    string emailString = email.Value; // Get string value
}
```

**Other provided value objects:**
- `EmailAddress`
- `Uri` (web addresses)
- More to come...

---

## ?? Extension Methods

### **ValidateToResult**

```csharp
// For InlineValidator
private static readonly InlineValidator<Order> s_validator = new() { /* rules */ };
return s_validator.ValidateToResult(order);

// For AbstractValidator
var validator = new OrderValidator();
return validator.ValidateToResult(order);
```

### **ToResult**

```csharp
// Convert value to Result
return order.ToResult();

// With Ensure
return order.ToResult()
    .Ensure(o => o.IsValid, Error.Validation("Invalid order"));
```

### **ToResultAsync**

```csharp
// Convert Task<T?> to Result<T>
return await _repository.GetByIdAsync(id, ct)
    .ToResultAsync(Error.NotFound($"Order {id} not found"));
```

---

## ?? Complete Controller Example

```csharp
namespace ProjectName.Api._2025_01_15.Controllers;

using Asp.Versioning;
using FunctionalDdd;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("2025-01-15")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    
    public OrdersController(ISender sender) => _sender = sender;
    
    [HttpGet("{id}")]
    public async ValueTask<ActionResult<OrderDto>> GetById(string id, CancellationToken ct)
        => await OrderId.TryCreate(id)
            .Bind(orderId => GetOrderByIdQuery.TryCreate(orderId))
            .BindAsync(query => _sender.Send(query, ct))
            .MapAsync(order => order.Adapt<OrderDto>())
            .ToActionResultAsync(this);
    
    [HttpPost]
    public async ValueTask<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest request, 
        CancellationToken ct)
        => await CustomerId.TryCreate(request.CustomerId)
            .Bind(customerId => CreateOrderCommand.TryCreate(customerId))
            .BindAsync(command => _sender.Send(command, ct))
            .MapAsync(order => order.Adapt<OrderDto>())
            .ToActionResultAsync(this);
    
    [HttpDelete("{id}")]
    public ActionResult<Unit> Delete(string id) =>
        OrderId.TryCreate(id).Match(
            ok => NoContent(),
            err => err.ToActionResult<Unit>(this));
}
```

---

## ?? Quick Reference

| What | API | Example |
|------|-----|---------|
| New ID | `OrderId.NewUnique()` | `var id = OrderId.NewUnique();` |
| ID from Guid | `OrderId.TryCreate(guid)` | `OrderId.TryCreate(existingGuid)` |
| ID from string | `OrderId.TryCreate(str)` | `OrderId.TryCreate("abc-123")` |
| Success | `Result.Success(value)` | `Result.Success(order)` |
| Success (Unit) | `Result.Success()` | `return Result.Success();` |
| Failure | `Result.Failure<T>(error)` | `Result.Failure<Order>(Error.Validation("msg"))` |
| Failure (Unit) | `Result.Failure(error)` | `return Result.Failure(Error.NotFound("msg"));` |
| Unit value | `Unit.Value` | Used with explicit type parameter |
| Error | `Error.Validation(msg)` | `Error.Validation("Required")` |
| Partial class | Required for RequiredGuid/RequiredString | `public partial class OrderId : RequiredGuid { }` |

---

**Package Reference:**
```xml
<PackageVersion Include="FunctionalDdd.RailwayOrientedProgramming" Version="3.0.0-alpha.3" />
<PackageVersion Include="FunctionalDdd.DomainDrivenDesign" Version="3.0.0-alpha.3" />
<PackageVersion Include="FunctionalDdd.CommonValueObjectGenerator" Version="3.0.0-alpha.3" />
<PackageVersion Include="FunctionalDdd.FluentValidation" Version="3.0.0-alpha.3" />
<PackageVersion Include="FunctionalDdd.CommonValueObjects" Version="3.0.0-alpha.3" />
<PackageVersion Include="FunctionalDdd.Asp" Version="3.0.0-alpha.3" />
