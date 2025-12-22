# Testing Patterns

## 🚨 MANDATORY Requirements

**Generate tests for EVERY layer:**
- Domain.Tests - **MUST contain actual test classes with [Fact] methods**
- Application.Tests - **MUST contain handler tests + Mock services**
- API.Tests (optional but recommended)

**DO NOT create empty test projects! Every test project MUST contain working test code.**

**Create mock services** in `Application.Tests/Mocks/` for ALL Application abstractions.

---

## 🧪 Domain Tests (MANDATORY - NOT EMPTY)

**MUST create test files for EVERY aggregate and value object**

Test aggregates, value objects, and behaviors:

```csharp
namespace ProjectName.Domain.Tests;

using FluentAssertions;
using Xunit;

public class OrderTests
{
    [Fact]
    public void CreateOrder_WithValidData_Succeeds()
    {
        // Arrange
        var customerId = CustomerId.NewUnique();
        
        // Act
        var result = Order.TryCreate(customerId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.Status.Should().Be(OrderStatus.Draft);
    }
    
    [Fact]
    public void CreateOrder_WithNullCustomerId_Fails()
    {
        // Arrange & Act
        var result = Order.TryCreate(null!);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
    
    [Fact]
    public void Submit_WhenDraft_Succeeds()
    {
        // Arrange
        var order = Order.TryCreate(CustomerId.NewUnique()).Value;
        
        // Act
        var result = order.Submit();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(OrderStatus.Submitted);
    }
    
    [Fact]
    public void Submit_WhenAlreadySubmitted_Fails()
    {
        // Arrange
        var order = Order.TryCreate(CustomerId.NewUnique()).Value;
        order.Submit();
        
        // Act
        var result = order.Submit();
        
        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
```

**For EACH aggregate, create tests for:**
- ✅ Successful creation with valid data
- ✅ Failed creation with invalid data
- ✅ Each public behavior method (success cases)
- ✅ Each public behavior method (failure cases)

---

## 🔧 Application Tests (MANDATORY - NOT EMPTY)

**MUST create test files for EVERY command handler and query handler**

Test commands, queries, and handlers using mocks:

```csharp
namespace ProjectName.Application.Tests.Commands;

using FluentAssertions;
using ProjectName.Application.Tests.Mocks;
using Xunit;

public class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesOrder()
    {
        // Arrange
        var mockRepo = new MockOrderRepository();
        var handler = new CreateOrderCommandHandler(mockRepo);
        var customerId = CustomerId.NewUnique();
        var command = CreateOrderCommand.TryCreate(customerId).Value;
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(customerId);
        
        var savedOrder = await mockRepo.GetByIdAsync(result.Value.Id);
        savedOrder.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_InvalidCustomerId_Fails()
    {
        // Arrange
        var mockRepo = new MockOrderRepository();
        var handler = new CreateOrderCommandHandler(mockRepo);
        
        // Act - This should fail at command creation
        var commandResult = CreateOrderCommand.TryCreate(null!);
        
        // Assert
        commandResult.IsFailure.Should().BeTrue();
    }
}
```

```csharp
namespace ProjectName.Application.Tests.Queries;

using FluentAssertions;
using ProjectName.Application.Tests.Mocks;
using Xunit;

public class GetOrderByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_OrderExists_ReturnsOrder()
    {
        // Arrange
        var mockRepo = new MockOrderRepository();
        var order = Order.TryCreate(CustomerId.NewUnique()).Value;
        await mockRepo.AddAsync(order);
        
        var handler = new GetOrderByIdQueryHandler(mockRepo);
        var query = GetOrderByIdQuery.TryCreate(order.Id).Value;
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
    }
    
    [Fact]
    public async Task Handle_OrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var mockRepo = new MockOrderRepository();
        var handler = new GetOrderByIdQueryHandler(mockRepo);
        var query = GetOrderByIdQuery.TryCreate(OrderId.NewUnique()).Value;
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
```

**For EACH handler, create tests for:**
- ✅ Successful operation
- ✅ Failure scenarios (not found, validation errors, etc.)

---

## 🎭 Mock Services (MANDATORY - NOT EMPTY)

**MANDATORY: Create a mock for EVERY Application abstraction**

Location: `Application.Tests/Mocks/`

### **Example: MockOrderRepository**
```csharp
namespace ProjectName.Application.Tests.Mocks;

using ProjectName.Application.Abstractions;
using ProjectName.Domain;

public class MockOrderRepository : IOrderRepository
{
    private readonly Dictionary<OrderId, Order> _orders = new();
    
    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }
    
    public Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_orders.Values.AsEnumerable());
    }
    
    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
    
    public Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(OrderId id, CancellationToken ct = default)
    {
        _orders.Remove(id);
        return Task.CompletedTask;
    }
    
    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
```

**For EACH repository/service interface:**
- ✅ Create a mock class that implements the interface
- ✅ Use in-memory storage (Dictionary, List, etc.)
- ✅ Implement all interface methods
- ✅ Make it work for testing - no real database/API calls

---

## ✅ Test File Organization

### Domain.Tests
```
Domain.Tests/
├── Aggregates/
│   ├── OrderTests.cs          ← Tests for Order aggregate
│   └── CustomerTests.cs       ← Tests for Customer aggregate
└── ValueObjects/
    ├── MoneyTests.cs          ← Tests for Money value object
    └── AddressTests.cs        ← Tests for Address value object
```

### Application.Tests
```
Application.Tests/
├── Commands/
│   ├── CreateOrderCommandHandlerTests.cs
│   └── UpdateOrderCommandHandlerTests.cs
├── Queries/
│   ├── GetOrderByIdQueryHandlerTests.cs
│   └── ListOrdersQueryHandlerTests.cs
└── Mocks/
    ├── MockOrderRepository.cs
    └── MockCustomerRepository.cs
```

---

## ✅ Benefits of Mock Services

- ✅ **No external dependencies** - Tests run without database
- ✅ **Fast execution** - In-memory operations
- ✅ **Predictable** - Control test data precisely
- ✅ **Isolation** - Test business logic independently
- ✅ **Repeatable** - Same results every time

---

## 📦 Test Project Configuration

Ensure `build/test.props` is imported:

```xml
<Project>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
```

---

## 🚨 CRITICAL REMINDERS

1. **DO NOT create empty test projects** - Every test project MUST have test classes
2. **Every aggregate MUST have tests** - At minimum: create success, create failure, each behavior
3. **Every handler MUST have tests** - At minimum: success case, failure case
4. **Every abstraction MUST have a mock** - Located in `Application.Tests/Mocks/`
5. **Tests MUST use FluentAssertions** - `.Should().BeTrue()`, `.Should().Be()`, etc.
6. **Tests MUST follow AAA pattern** - Arrange, Act, Assert with comments
