# Domain Layer Patterns

## 🚨 CRITICAL: Use FunctionalDDD Library Classes

**DO NOT implement your own base classes!**

**Official Documentation**: https://github.com/xavierjohn/FunctionalDDD

All value objects and aggregates **MUST use** FunctionalDDD library classes:
- ✅ `RequiredGuid` from `FunctionalDdd.CommonValueObjectGenerator`
- ✅ `RequiredString` from `FunctionalDdd.CommonValueObjectGenerator`
- ✅ `Aggregate<TId>` from `FunctionalDdd.DomainDrivenDesign`
- ✅ `ScalarValueObject<T>` from `FunctionalDdd.DomainDrivenDesign`
- ✅ `ValueObject` from `FunctionalDdd.DomainDrivenDesign`
- ✅ `Result<T>` from `FunctionalDdd.RailwayOrientedProgramming`
- ✅ `InlineValidator` from `FunctionalDdd.FluentValidation`

**Required using statement:**
```csharp
using FunctionalDdd;
```

This automatically imports all FunctionalDDD types.

---

## 🎯 Value Objects

### **ID Types - Use RequiredGuid (from library!)**
```csharp
// Uses source generator from FunctionalDdd.CommonValueObjectGenerator
public partial class OrderId : RequiredGuid { }
public partial class CustomerId : RequiredGuid { }
public partial class ProductId : RequiredGuid { }
```

**❌ DO NOT create custom RequiredGuid class!** Use the library version.

### **String Types - Use RequiredString (from library!)**
```csharp
// Uses source generator from FunctionalDdd.CommonValueObjectGenerator
public partial class Title : RequiredString { }
public partial class ProductName : RequiredString { }
```

**❌ DO NOT create custom RequiredString class!** Use the library version.

### **Library-Provided Types**
```csharp
// EmailAddress is provided by FunctionalDDD.CommonValueObjects
// Just use it - don't implement!
using FunctionalDdd;

var emailResult = EmailAddress.TryCreate("user@example.com");
```

### **Custom Value Objects - ScalarValueObject (from library!)**
For single-value types with custom validation:

```csharp
// Inherits from FunctionalDdd.DomainDrivenDesign.ScalarValueObject<T>
public class ZipCode : ScalarValueObject<string>
{
    private ZipCode(string value) : base(value) { }
    
    public static Result<ZipCode> TryCreate(string? value)
    {
        return value.ToResult()
            .Ensure(v => !string.IsNullOrWhiteSpace(v), 
                   Error.Validation("Zip code cannot be empty"))
            .Ensure(v => Regex.IsMatch(v, @"^\d{5}(?:[-\s]\d{4})?$"), 
                   Error.Validation("Invalid US zip code format"))
            .Map(v => new ZipCode(v));
    }
}
```

### **Complex Value Objects - ValueObject (from library!)**
For multi-property types:

```csharp
// Inherits from FunctionalDdd.DomainDrivenDesign.ValueObject
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Result<Money> TryCreate(decimal amount, string currency = "USD")
    {
        return (amount, currency).ToResult()
            .Ensure(x => x.amount >= 0, Error.Validation("Amount cannot be negative"))
            .Ensure(x => x.currency.Length == 3, Error.Validation("Invalid currency code"))
            .Map(x => new Money(x.amount, x.currency));
    }
    
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

---

## 🏛️ Aggregates (Use library Aggregate<TId>!)

### **Recommended Pattern - AbstractValidator as Nested Class**

Based on the FunctionalDddAspTemplate, use `AbstractValidator<T>` as a **nested class** for aggregates:

```csharp
// Inherits from FunctionalDdd.DomainDrivenDesign.Aggregate<TId>
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
- ✅ Use `AbstractValidator<T>` as **nested class** (not InlineValidator)
- ✅ Create **new instance** of validator in `TryCreate()`
- ✅ Call `validator.ValidateToResult(aggregate)` - NOT `s_validator.ValidateToResult()`
- ✅ Suitable for both simple and complex validation

**❌ DO NOT create custom Aggregate<TId> base class!** Use the library version.

### **Alternative - InlineValidator (for very simple cases)**

Only use for aggregates with 1-2 simple validation rules:

```csharp
public class SimpleEntity : Aggregate<SimpleEntityId>
{
    public Name Name { get; }
    
    public static Result<SimpleEntity> TryCreate(Name name)
    {
        var entity = new SimpleEntity(name);
        return s_validator.ValidateToResult(entity);
    }
    
    private SimpleEntity(Name name) : base(SimpleEntityId.NewUnique())
    {
        Name = name;
    }
    
    private static readonly InlineValidator<SimpleEntity> s_validator = new()
    {
        v => v.RuleFor(x => x.Name).NotNull()
    };
}
```

**Note:** The FunctionalDddAspTemplate uses `AbstractValidator` for aggregates. Use `InlineValidator` only if you have a specific reason to deviate from the template.

---

## 📢 Domain Events

```csharp
public record OrderCreatedEvent(OrderId OrderId, CustomerId CustomerId) 
    : DomainEvent;

public record OrderSubmittedEvent(OrderId OrderId) 
    : DomainEvent;
```

---

## ✅ Guidelines

- ✅ **Use library-provided base classes** - Never create your own!
- ✅ **Aggregates**: Use `AbstractValidator<T>` as nested class (template pattern)
- ✅ **Commands/Queries**: Use `InlineValidator` as static field
- ✅ **Value Objects**: Use `InlineValidator` for ScalarValueObject with validation
- ✅ Use `RequiredGuid` for all ID types (from library)
- ✅ Use `RequiredString` for simple required strings (from library)
- ✅ Use library `EmailAddress` - don't implement
- ✅ Use `ScalarValueObject<T>` for single-value custom types (from library)
- ✅ Use `ValueObject` for multi-property types (from library)
- ✅ Use `Aggregate<TId>` for aggregates (from library)
- ✅ Add domain events for significant state changes

**See:** [Template Reference](template-reference.md) for complete examples from FunctionalDddAspTemplate
