# Railway-Oriented Programming Patterns

## 🚨 CRITICAL RULES (MANDATORY)

### **NEVER Check Result Status in Controllers**

**NEVER** check `IsFailed`, `IsFailure`, or `IsSuccess` in controller methods!

❌ **WRONG - Do NOT do this:**
```csharp
var result = ValueObject.TryCreate(input);
if (result.IsFailure)  // ❌ BREAKS THE RAILWAY!
    return BadRequest();
```

✅ **CORRECT - ALWAYS chain methods:**
```csharp
=> await ValueObject.TryCreate(input)
    .Bind(value => Command.TryCreate(value))
    .BindAsync(cmd => _sender.Send(cmd, ct))
    .MapAsync(result => result.Adapt<Dto>())
    .ToActionResultAsync(this);
```

---

## 🔗 Railway Operators Reference

| Operator | Purpose | Use When |
|----------|---------|----------|
| `Bind()` | Chain sync operations returning `Result<T>` | Combining validation steps |
| `BindAsync()` | Chain async operations returning `Task<Result<T>>` | Database calls, API calls |
| `Map()` | Transform success value (sync) | Convert types |
| `MapAsync()` | Transform success value (async) | Async transformations |
| `Tap()` | Side effects without changing result (sync) | Logging, notifications |
| `TapAsync()` | Side effects without changing result (async) | Saving to database |
| `Ensure()` | Add inline validation | Business rule checks |
| `Combine()` | Merge multiple Results | Multiple inputs validation |
| `Match()` | Pattern match success/failure | Final result handling |
| `ToActionResultAsync()` | Convert to HTTP response | Controller endpoints |

---

## 📋 Common Patterns

### **Single Value Creation**
```csharp
=> await OrderId.TryCreate(id)
    .Bind(orderId => GetOrderByIdQuery.TryCreate(orderId))
    .BindAsync(query => _sender.Send(query, ct))
    .ToActionResultAsync(this);
```

### **Multiple Values with Combine**
```csharp
=> await OrderId.TryCreate(id)
    .Combine(CustomerId.TryCreate(customerId))
    .Bind((orderId, customerId) => UpdateOrderCommand.TryCreate(orderId, customerId))
    .BindAsync(command => _sender.Send(command, ct))
    .ToActionResultAsync(this);
```

### **With Transformation**
```csharp
=> await OrderId.TryCreate(id)
    .BindAsync(orderId => _repository.GetByIdAsync(orderId, ct))
    .MapAsync(order => order.Adapt<OrderDto>())
    .ToActionResultAsync(this);
```

### **With Side Effects**
```csharp
=> await Order.TryCreate(customerId)
    .TapAsync(order => _repository.AddAsync(order, ct))
    .TapAsync(_ => _repository.SaveChangesAsync(ct))
    .MapAsync(order => order.Adapt<OrderDto>())
    .ToActionResultAsync(this);
```

### **Using Match for Custom Responses**
```csharp
public ActionResult Delete(string id) =>
    OrderId.TryCreate(id).Match(
        ok => NoContent(),
        err => err.ToActionResult<Unit>(this));
```

---

## ⚠️ Anti-Patterns to Avoid

### ❌ Checking IsFailure
```csharp
// NEVER DO THIS
var result = Order.TryCreate(customerId);
if (result.IsFailure)
    return BadRequest(result.Error);
return Ok(result.Value);
```

### ❌ Accessing .Value Without Checking
```csharp
// DANGEROUS - Can throw if failed
var order = Order.TryCreate(customerId).Value;  // ❌
```

### ❌ Breaking the Chain
```csharp
// WRONG - Imperative style
var idResult = OrderId.TryCreate(id);
var queryResult = GetOrderByIdQuery.TryCreate(idResult.Value);
var order = await _sender.Send(queryResult.Value, ct);
```

---

## ✅ Best Practices

1. **Always return the chain** - Use `=> await` for single expression
2. **Never access .Value directly** - Let the railway handle it
3. **Use Bind for Result-returning operations** - Keeps you on the rails
4. **Use Map for transformations** - When you just need to convert types
5. **Use Tap for side effects** - When you need to do something but not change the value
6. **End with ToActionResultAsync()** - Converts Result to HTTP response automatically
