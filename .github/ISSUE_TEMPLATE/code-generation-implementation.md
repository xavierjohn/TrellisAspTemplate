---
name: Code Generation Implementation
about: Track the implementation of full code generation in scaffolding workflow
title: 'feat: Implement full code generation in scaffolding workflow'
labels: enhancement, code-generation, high-priority
assignees: ''
---

## ?? Objective

Implement full code generation in the `copilot-scaffold.yml` workflow to create a complete Clean Architecture project structure based on the project specification YAML file.

## ?? Current State

? **Working Infrastructure**:
- Workflow triggers on `copilot-scaffold` label
- Reads and parses `.github/project-spec.yml` using yq
- Creates feature branch
- Commits and pushes changes
- Adds status comments to issues

?? **Current Limitation**:
The workflow only generates a basic README.md file. It does NOT generate the actual project structure.

**Test Evidence**: 
- Issue #7 in FunctionalDDD-Agent-Test repository
- Branch: `scaffold-7-TodoList` contains only README.md

## ?? Required Implementation

### Phase 1: Copy Template Structure (MVP)

**Goal**: Copy existing template files and update namespaces

#### Tasks:
- [ ] Add step to copy template structure from repository
- [ ] Implement namespace replacement logic
  - Replace `FunctionalDddAspTemplate` with `spec.project.namespace`
  - Update all .csproj files
  - Update solution file
  - Update using statements
- [ ] Test with minimal project spec
- [ ] Verify solution builds successfully

**Acceptance Criteria**:
- ? Workflow copies all template files (Domain, Application, ACL, API)
- ? Namespaces are correctly updated throughout
- ? Solution file references correct project names
- ? `dotnet build` succeeds on generated code

---

### Phase 2: Domain Layer Generation

**Goal**: Generate domain aggregates and value objects from spec

#### Based on `spec.domain.aggregates`:
- [ ] Generate aggregate classes
  - [ ] Create class file with proper namespace
  - [ ] Add properties from spec
  - [ ] Implement `TryCreate()` factory method
  - [ ] Add FluentValidation rules
  - [ ] Implement behaviors as methods
  - [ ] Add domain event tracking
- [ ] Generate tests for each aggregate
  - [ ] Test `TryCreate()` validation
  - [ ] Test behavior methods
  - [ ] Test domain events

#### Based on `spec.domain.valueObjects`:
- [ ] Generate value object classes
  - [ ] Use `RequiredGuid` for ID types
  - [ ] Use `RequiredString` for simple strings
  - [ ] Use `ScalarValueObject<T>` for single-value types
  - [ ] Use `ValueObject` for multi-property types
- [ ] Generate tests for value objects

**Example Generation**:
```yaml
# Input (project-spec.yml)
domain:
  aggregates:
    - name: TodoItem
      id: TodoItemId
      properties:
        - name: Title
          type: TodoTitle
          required: true
        - name: IsCompleted
          type: bool
      behaviors:
        - name: Complete
          description: Mark as complete
```

```csharp
// Output (Domain/src/Aggregates/TodoItem.cs)
public class TodoItem : Aggregate<TodoItemId>
{
    public TodoTitle Title { get; }
    public bool IsCompleted { get; private set; }
    
    private TodoItem(TodoItemId id, TodoTitle title) : base(id)
    {
        Title = title;
        IsCompleted = false;
        DomainEvents.Add(new TodoItemCreatedEvent(id, title));
    }
    
    public static Result<TodoItem> TryCreate(TodoTitle title)
    {
        var item = new TodoItem(TodoItemId.NewUnique(), title);
        return s_validator.ValidateToResult(item);
    }
    
    public Result<TodoItem> Complete()
    {
        return this.ToResult()
            .Ensure(_ => !IsCompleted, Error.Validation("Already completed"))
            .Tap(_ => IsCompleted = true);
    }
    
    private static readonly InlineValidator<TodoItem> s_validator = new()
    {
        v => v.RuleFor(x => x.Title).NotNull()
    };
}
```

**Acceptance Criteria**:
- ? All aggregates from spec are generated
- ? All value objects from spec are generated
- ? Generated code follows FunctionalDDD patterns
- ? All domain tests are generated and pass

---

### Phase 3: Application Layer Generation

**Goal**: Generate CQRS queries, commands, and handlers

#### Based on `spec.application.queries`:
- [ ] Generate query classes
  - [ ] Create query with parameters
  - [ ] Add FluentValidation
  - [ ] Implement `TryCreate()` method
- [ ] Generate query handlers
  - [ ] Implement `IQueryHandler<TQuery, TResult>`
  - [ ] Add repository dependencies
  - [ ] Implement handler logic
- [ ] Generate query tests

#### Based on `spec.application.commands`:
- [ ] Generate command classes
  - [ ] Create command with parameters
  - [ ] Add FluentValidation
  - [ ] Implement `TryCreate()` method
- [ ] Generate command handlers
  - [ ] Implement `ICommandHandler<TCommand, TResult>`
  - [ ] Add repository dependencies
  - [ ] Implement handler logic
  - [ ] Publish domain events
- [ ] Generate command tests

**Example Generation**:
```yaml
# Input (project-spec.yml)
application:
  commands:
    - name: CreateTodoItem
      parameters:
        - title: TodoTitle
      returns: TodoItem
```

```csharp
// Output (Application/src/Commands/CreateTodoItemCommand.cs)
public class CreateTodoItemCommand : ICommand<Result<TodoItem>>
{
    public TodoTitle Title { get; }
    
    private CreateTodoItemCommand(TodoTitle title) => Title = title;
    
    public static Result<CreateTodoItemCommand> TryCreate(TodoTitle title)
        => s_validator.ValidateToResult(new CreateTodoItemCommand(title));
    
    private static readonly InlineValidator<CreateTodoItemCommand> s_validator = new()
    {
        v => v.RuleFor(x => x.Title).NotNull()
    };
}

// Output (Application/src/Commands/CreateTodoItemCommandHandler.cs)
public class CreateTodoItemCommandHandler 
    : ICommandHandler<CreateTodoItemCommand, Result<TodoItem>>
{
    private readonly ITodoItemRepository _repository;
    
    public CreateTodoItemCommandHandler(ITodoItemRepository repository) 
        => _repository = repository;
    
    public async ValueTask<Result<TodoItem>> Handle(
        CreateTodoItemCommand command, 
        CancellationToken ct)
        => await TodoItem.TryCreate(command.Title)
            .BindAsync(item => _repository.AddAsync(item, ct));
}
```

**Acceptance Criteria**:
- ? All queries from spec are generated
- ? All commands from spec are generated
- ? All handlers are generated and registered with Mediator
- ? All application tests are generated and pass

---

### Phase 4: ACL Layer Generation

**Goal**: Generate repository interfaces and implementations

#### Tasks:
- [ ] Generate repository interfaces
  - [ ] One interface per aggregate
  - [ ] CRUD methods
  - [ ] Custom query methods
- [ ] Generate repository implementations
  - [ ] In-memory implementations for testing
  - [ ] Or database implementations (e.g., Entity Framework)
- [ ] Update DependencyInjection.cs
  - [ ] Register all repositories
- [ ] Generate repository tests

**Example Generation**:
```csharp
// Output (Application/src/Abstractions/ITodoItemRepository.cs)
public interface ITodoItemRepository
{
    ValueTask<TodoItem?> GetByIdAsync(TodoItemId id, CancellationToken ct);
    ValueTask<List<TodoItem>> GetAllAsync(CancellationToken ct);
    ValueTask<TodoItem> AddAsync(TodoItem item, CancellationToken ct);
    ValueTask<TodoItem> UpdateAsync(TodoItem item, CancellationToken ct);
}

// Output (ACL/src/Repositories/TodoItemRepository.cs)
public class TodoItemRepository : ITodoItemRepository
{
    private readonly Dictionary<TodoItemId, TodoItem> _items = new();
    
    public ValueTask<TodoItem?> GetByIdAsync(TodoItemId id, CancellationToken ct)
        => ValueTask.FromResult(_items.GetValueOrDefault(id));
    
    // ... other methods
}
```

**Acceptance Criteria**:
- ? Repository interface generated for each aggregate
- ? Repository implementation generated
- ? Repositories registered in DI container
- ? Repository tests generated and pass

---

### Phase 5: API Layer Generation

**Goal**: Generate controllers with Railway-Oriented Programming

#### Based on `spec.api.endpoints`:
- [ ] Generate controllers
  - [ ] One controller per resource
  - [ ] Implement specified operations (GET, POST, PUT, DELETE)
  - [ ] Use Railway-Oriented Programming patterns
  - [ ] Add proper status codes
  - [ ] Add XML documentation comments
- [ ] Generate DTOs
  - [ ] Request DTOs
  - [ ] Response DTOs
  - [ ] Mapster configuration
- [ ] Place in date-versioned folders (`YYYY-MM-DD/`)
- [ ] Generate API tests

**Example Generation**:
```yaml
# Input (project-spec.yml)
api:
  version: "2025-01-15"
  endpoints:
    - resource: todos
      operations: [GET, POST, PUT]
```

```csharp
// Output (Api/src/2025-01-15/Controllers/TodosController.cs)
[ApiController]
[ApiVersion("2025-01-15")]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ISender _sender;
    
    public TodosController(ISender sender) => _sender = sender;
    
    /// <summary>
    /// Get all todo items.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TodoItemDto>), StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<List<TodoItemDto>>> GetAll(CancellationToken ct)
        => await GetAllTodoItemsQuery.TryCreate()
            .BindAsync(query => _sender.Send(query, ct))
            .MapAsync(items => items.Adapt<List<TodoItemDto>>())
            .ToActionResultAsync(this);
    
    /// <summary>
    /// Create a new todo item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TodoItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<TodoItemDto>> Create(
        [FromBody] CreateTodoItemRequest request,
        CancellationToken ct)
        => await TodoTitle.TryCreate(request.Title)
            .Bind(CreateTodoItemCommand.TryCreate)
            .BindAsync(cmd => _sender.Send(cmd, ct))
            .MapAsync(item => item.Adapt<TodoItemDto>())
            .ToActionResultAsync(this);
}
```

**Acceptance Criteria**:
- ? Controllers generated for all endpoints
- ? All specified operations implemented
- ? Railway-Oriented Programming patterns used
- ? DTOs generated
- ? Swagger documentation includes all endpoints
- ? API tests generated and pass

---

### Phase 6: Integration & Testing

**Goal**: Ensure generated code works end-to-end

#### Tasks:
- [ ] Verify solution builds without errors
- [ ] Run all generated tests
- [ ] Start API and verify Swagger UI
- [ ] Test API endpoints manually
- [ ] Verify OpenTelemetry integration
- [ ] Check code quality
  - [ ] No compiler warnings
  - [ ] Consistent formatting
  - [ ] Proper XML documentation

**Acceptance Criteria**:
- ? `dotnet build` succeeds
- ? `dotnet test` shows all tests passing
- ? API starts successfully
- ? Swagger UI displays all endpoints
- ? Manual API tests pass
- ? No warnings or errors

---

## ?? Implementation Approach

### Recommended: Separate Code Generation Tool

Create a new .NET tool project:

```
FunctionalDddAspTemplate/
??? tools/
?   ??? CodeGenerator/
?       ??? CodeGenerator.csproj
?       ??? Program.cs
?       ??? Generators/
?       ?   ??? DomainGenerator.cs
?       ?   ??? ApplicationGenerator.cs
?       ?   ??? AclGenerator.cs
?       ?   ??? ApiGenerator.cs
?       ??? Templates/
?           ??? Aggregate.cs.template
?           ??? ValueObject.cs.template
?           ??? Command.cs.template
?           ??? Controller.cs.template
```

**Workflow Integration**:
```yaml
- name: Install Code Generator
  run: dotnet tool install --global FunctionalDDD.CodeGenerator

- name: Generate Project Structure
  run: |
    code-generator generate \
      --spec .github/project-spec.yml \
      --output . \
      --template-path ${{ github.workspace }}
```

**Benefits**:
- ? Testable code generation logic
- ? Reusable outside of GitHub Actions
- ? Easy to debug and iterate
- ? Can be distributed as NuGet tool

---

## ?? Success Metrics

When this issue is complete:

- [ ] Workflow generates complete Clean Architecture structure
- [ ] Generated code follows all FunctionalDDD patterns
- [ ] All layers generated: Domain, Application, ACL, API
- [ ] All tests generated and passing
- [ ] Solution builds successfully
- [ ] API runs and serves Swagger UI
- [ ] Documentation updated to reflect full capabilities

---

## ?? Related

- Original migration issue: (link to your issue)
- Test repository: https://github.com/xavierjohn/FunctionalDDD-Agent-Test
- Proof of concept: Issue #7, Branch `scaffold-7-TodoList`

---

## ?? Notes

This is a significant feature implementation that will transform the template from a proof-of-concept into a production-ready scaffolding tool.

**Estimated Effort**: Medium to Large (several weeks)

**Priority**: High - This is the key differentiator for the template

**Dependencies**: None - infrastructure is already in place
