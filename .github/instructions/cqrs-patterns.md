# CQRS Patterns

## 📋 Queries

### **Query Definition**
```csharp
public class GetOrderByIdQuery : IQuery<Result<Order>>
{
    public OrderId OrderId { get; }
    
    public static Result<GetOrderByIdQuery> TryCreate(OrderId orderId)
        => s_validator.ValidateToResult(new GetOrderByIdQuery(orderId));
    
    private GetOrderByIdQuery(OrderId orderId) => OrderId = orderId;
    
    private static readonly InlineValidator<GetOrderByIdQuery> s_validator = new()
    {
        v => v.RuleFor(x => x.OrderId).NotNull()
    };
}
```

### **Query Handler**
```csharp
public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Result<Order>>
{
    private readonly IOrderRepository _repository;
    
    public GetOrderByIdQueryHandler(IOrderRepository repository) 
        => _repository = repository;
    
    public async ValueTask<Result<Order>> Handle(
        GetOrderByIdQuery query, 
        CancellationToken ct)
        => await _repository.GetByIdAsync(query.OrderId, ct)
            .ToResultAsync(Error.NotFound($"Order {query.OrderId} not found"));
}
```

---

## ✏️ Commands

### **Command Definition**
```csharp
public class CreateOrderCommand : ICommand<Result<Order>>
{
    public CustomerId CustomerId { get; }
    
    public static Result<CreateOrderCommand> TryCreate(CustomerId customerId)
        => s_validator.ValidateToResult(new CreateOrderCommand(customerId));
    
    private CreateOrderCommand(CustomerId customerId) => CustomerId = customerId;
    
    private static readonly InlineValidator<CreateOrderCommand> s_validator = new()
    {
        v => v.RuleFor(x => x.CustomerId).NotNull()
    };
}
```

### **Command Handler**
```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<Order>>
{
    private readonly IOrderRepository _repository;
    
    public CreateOrderCommandHandler(IOrderRepository repository) 
        => _repository = repository;
    
    public async ValueTask<Result<Order>> Handle(
        CreateOrderCommand command, 
        CancellationToken ct)
        => await Order.TryCreate(command.CustomerId)
            .TapAsync(order => _repository.AddAsync(order, ct))
            .TapAsync(_ => _repository.SaveChangesAsync(ct));
}
```

---

## 🔌 Abstractions

Define repository interfaces in `Application/src/Abstractions/`:

```csharp
namespace ProjectName.Application.Abstractions;

using ProjectName.Domain;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task DeleteAsync(OrderId id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

---

## ⚙️ Dependency Injection

```csharp
namespace ProjectName.Application;

using Mediator;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options => 
            options.ServiceLifetime = ServiceLifetime.Scoped);
        
        return services;
    }
}
```

---

## ✅ Guidelines

- ✅ Use `InlineValidator` for queries and commands
- ✅ Return `Result<T>` from handlers
- ✅ Use `TapAsync` for side effects (saving data)
- ✅ Define repository interfaces in Application layer
- ✅ Implement repositories in ACL layer
- ✅ Use Mediator for CQRS pipeline
