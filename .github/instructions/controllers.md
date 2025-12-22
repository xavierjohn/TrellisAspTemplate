# API Controller Patterns

## 🚨 MANDATORY Requirements

### **API Versioning**
- **Namespace**: `_2025_01_15` (underscores)
- **Folder**: `2025-01-15` (hyphens)
- **Attribute**: `[ApiVersion("2025-01-15")]` (current date in YYYY-MM-DD format)
- **Route**: `[Route("api/[controller]")]` (NO version in route)

### **Required Files**
**MUST create:**
1. `Api/src/Properties/launchSettings.json`
2. `Api/src/api.http`

---

## 📋 Complete Controller Template

```csharp
namespace ProjectName.Api._2025_01_15.Controllers;

using Asp.Versioning;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Order management endpoints.
/// </summary>
[ApiController]
[ApiVersion("2025-01-15")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    
    public OrdersController(ISender sender) => _sender = sender;
    
    /// <summary>
    /// Get order by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<OrderDto>> GetById(string id, CancellationToken ct)
        => await OrderId.TryCreate(id)
            .Bind(orderId => GetOrderByIdQuery.TryCreate(orderId))
            .BindAsync(query => _sender.Send(query, ct))
            .MapAsync(order => order.Adapt<OrderDto>())
            .ToActionResultAsync(this);
    
    /// <summary>
    /// Create a new order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest request, 
        CancellationToken ct)
        => await CustomerId.TryCreate(request.CustomerId)
            .Bind(customerId => CreateOrderCommand.TryCreate(customerId))
            .BindAsync(command => _sender.Send(command, ct))
            .MapAsync(order => order.Adapt<OrderDto>())
            .ToActionResultAsync(this);
    
    /// <summary>
    /// Update an existing order.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<OrderDto>> Update(
        string id, 
        [FromBody] UpdateOrderRequest request, 
        CancellationToken ct)
        => await OrderId.TryCreate(id)
            .Combine(CustomerId.TryCreate(request.CustomerId))
            .Bind((orderId, customerId) => UpdateOrderCommand.TryCreate(orderId, customerId))
            .BindAsync(command => _sender.Send(command, ct))
            .MapAsync(order => order.Adapt<OrderDto>())
            .ToActionResultAsync(this);
    
    /// <summary>
    /// Delete an order.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Unit> Delete(string id) =>
        OrderId.TryCreate(id).Match(
            ok => NoContent(),
            err => err.ToActionResult<Unit>(this));
}
```

---

## 📂 File Structure

```
Api/src/
├── 2025-01-15/              ← Use hyphens in folder name
│   ├── Controllers/
│   │   └── OrdersController.cs
│   └── Models/
│       ├── OrderDto.cs
│       ├── CreateOrderRequest.cs
│       └── UpdateOrderRequest.cs
├── Properties/
│   └── launchSettings.json  ← Required
├── api.http                 ← Required
└── Program.cs
```

**Namespace uses underscores**: `ProjectName.Api._2025_01_15.Controllers`

---

## 🎨 DTOs and Mapping

### **Request Models**
```csharp
namespace ProjectName.Api._2025_01_15.Models;

public record CreateOrderRequest(string CustomerId);
public record UpdateOrderRequest(string CustomerId);
```

### **Response Models**
```csharp
public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status);
```

### **Mapster Configuration**
```csharp
namespace ProjectName.Api._2025_01_15.Models;

using Mapster;
using ProjectName.Domain;

public class ConfigureMapster : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Order, OrderDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CustomerId, src => src.CustomerId.Value)
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
```
