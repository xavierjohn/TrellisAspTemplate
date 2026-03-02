# Copilot Instructions — Building with Trellis

This project uses the **Trellis** framework (.NET 10). Trellis combines Railway-Oriented Programming (ROP) with Domain-Driven Design (DDD). Follow these patterns exactly.

**API Reference:** See `.github/trellis-api-reference.md` for all Trellis types, method signatures, and usage patterns. Use it as the authoritative source for Trellis API surface.

## Core Principles

1. **Errors are values, not exceptions.** Use `Result<T>` for expected failures. Never throw for business logic. Never use try/catch in Domain or Application layers.
2. **Make illegal states unrepresentable.** Every domain concept is a value object with `TryCreate`. If it exists, it's valid.
3. **No primitive obsession.** No raw `Guid`, `string`, or `int` in domain method signatures. Use typed value objects everywhere.
4. **Optional values use `Maybe<T>`, never null.** `Maybe<PhoneNumber>`, not `PhoneNumber?`.

## Architecture

```
Api → Application → Domain
Api → Acl → Application → Domain
```

| Layer | Depends On | Contains |
|-------|-----------|----------|
| **Domain** | Trellis packages only (Results, Primitives, DDD, Stateless, Authorization) | Aggregates, entities, value objects, domain events, specifications, permission constants |
| **Application** | Domain, Mediator, Trellis.Mediator | Commands, queries, handlers, repository interfaces |
| **Acl** | Application, Trellis.EntityFrameworkCore, EF Core provider | DbContext, entity configurations, repository implementations, migrations |
| **Api** | Application, Acl, Trellis.Asp | Endpoints, DTOs, Program.cs (composition root), IActorProvider implementation |

> **Why "Acl"?** ACL stands for Anti-Corruption Layer. This avoids confusion with actual infrastructure (servers, databases, cloud services). The Acl layer adapts external systems (SQL Server, message queues, etc.) to the domain model through repository implementations and EF Core.

**Rules:**
- Domain has ZERO external dependencies (no EF Core, no ASP.NET, no Mediator).
- Repository interfaces live in Application, implementations in Acl.
- `Mediator.SourceGenerator` is installed in the **Application** project (where commands and queries are defined).
- Each layer has one `DependencyInjection.cs` with an `Add{Layer}()` extension method.
- Register `IActorProvider` as **singleton** in the Api layer. This is safe because `IHttpContextAccessor.HttpContext` uses `AsyncLocal` internally. Trellis pipeline behaviors are registered as singletons, so a scoped `IActorProvider` will cause a runtime exception.

## Project Layout

The template provides the complete project structure. Do NOT modify or recreate build system files (`Directory.Build.props`, `Directory.Packages.props`, `global.json`, `build/test.props`). They are pre-configured.

```
{ServiceName}/
├── {ServiceName}.slnx
├── Directory.Build.props          ← DO NOT MODIFY
├── Directory.Packages.props       ← ADD new packages here (versions only)
├── global.json                    ← DO NOT MODIFY
├── build/
│   └── test.props                 ← DO NOT MODIFY
├── .github/
│   ├── copilot-instructions.md    ← this file
│   └── trellis-api-reference.md   ← Trellis API surface
├── Domain/
│   ├── src/
│   │   └── Domain.csproj
│   └── tests/
│       └── Domain.Tests.csproj
├── Application/
│   ├── src/
│   │   └── Application.csproj
│   └── tests/
│       └── Application.Tests.csproj
├── Acl/
│   ├── src/
│   │   └── AntiCorruptionLayer.csproj
│   └── tests/
│       └── AntiCorruptionLayer.Tests.csproj
└── Api/
    ├── src/
    │   └── Api.csproj
    └── tests/
        └── Api.Tests.csproj
```

**Adding NuGet packages:** Add `<PackageVersion>` entries to `Directory.Packages.props`, then `<PackageReference>` (without version) in the relevant `.csproj`. Never specify versions in `.csproj` files.

**HTTP file:** The template includes `Api/src/api.http` with sample requests. After implementing the spec, **replace its contents** with requests covering every endpoint in the API — happy-path examples, error cases, and the full resource lifecycle. Use `@variables` for host, api-version, and response-chained IDs (e.g., `{{createCustomer.response.body.id}}`). This file is the living documentation for manual testing and onboarding.

## Key Conventions

### Commands and Queries

- Commands receive **value object types** (e.g., `CustomerId`, not `Guid`). Scalar value binding validates at the API layer — handlers never call `TryCreate` on command properties.
- Use `IValidate` **only** for cross-field or collection validation (e.g., "at least one line item"). Single-field validation is handled by value objects.
- Use `IAuthorize` for permission-based authorization. Use `IAuthorizeResource` for resource-based authorization (e.g., "only the owner can cancel").
- **`IAuthorizeResource` timing:** The `Authorize(Actor)` method runs in the pipeline *before* the handler, so the resource hasn't been loaded yet. For ownership checks that require the entity, implement `Authorize` as `Result.Success()` and perform the actual resource-based check inside the handler after loading the entity.
- **`Unit` type disambiguation:** Both `Trellis` and `Mediator` define a `Unit` type. In handler return types and ROP chains, always use `Trellis.Unit` (or `default(Trellis.Unit)`). The global `using Trellis;` directive makes the unqualified `Unit` resolve to `Trellis.Unit`, but when both namespaces are imported, qualify explicitly.

### EF Core

- **NEVER write `HasConversion()`.** Call `ApplyTrellisConventions` in `ConfigureConventions` — it handles all scalar Trellis value objects and `Money` properties automatically.
- **Custom composite `ValueObject` types** (e.g., `ShippingAddress` with multiple fields) are NOT auto-mapped by `ApplyTrellisConventions`. Map them with `OwnsOne` in `OnModelCreating` and configure each property explicitly. `Money` is the exception — it IS auto-mapped.
- Use `SaveChangesResultAsync` (not `SaveChangesAsync` directly).
- Use `FirstOrDefaultMaybeAsync` for optional lookups, `FirstOrDefaultResultAsync` for required lookups.
- Use `.Where(specification)` for specification queries.
- **`Maybe<T>` properties** require the backing-field pattern with `MaybeProperty` in `OnModelCreating`. Use `WhereNone`, `WhereHasValue`, `WhereEquals` for LINQ queries on those properties. See §12 in `trellis-api-reference.md`.

### MVC Controllers

Controllers inherit `ControllerBase` with `[ApiController]`. Actions are thin — send command via Mediator, chain `.ToActionResult(this)` or `.ToActionResultAsync(this)`.

**Every controller must have:**
- `[ApiController]` attribute and inherit `ControllerBase`
- `[ApiVersion("2026-11-12")]` at class level (use date-based versions)
- `[ServiceLevelIndicator]` at class level
- `[Route("api/[controller]")]` at class level
- `[Consumes("application/json")]` and `[Produces("application/json")]` at class level
- Error responses as RFC 9457 Problem Details (handled by `ToActionResult`)

**Use `ToCreatedAtActionResult`** for POST endpoints that create resources — returns `201 Created` with `Location` header.

### Automatic Scalar Value Binding

**Use value object types — not primitives — in controller action parameters.** Trellis automatically converts route parameters, query parameters, and JSON body properties via model binding and JSON converters. Never call `.Create()` or `.TryCreate()` manually in controllers.

**Registration** — add scalar value validation to the MVC pipeline in `Api/src/DependencyInjection.cs`:
```csharp
services.AddControllers().AddScalarValueValidation();
```
And activate the middleware in `Program.cs`:
```csharp
app.UseScalarValueValidation();
```

**Request/Response DTOs** live in `Api/src/Contracts/`. Never expose domain types directly. Request DTOs can use scalar value object types as properties — they will be validated automatically via the JSON converter.

**API Versioning** — Controllers are organized in versioned folders under `Api/src/` (e.g., `Api/src/2026-11-12/Controllers/`).

## Testing Strategy

**Domain tests:** Pure unit tests, no external dependencies. Test value object TryCreate, aggregate rules, state machine transitions, specifications.

**Application tests:** Mock repository interfaces. Test handler logic, authorization checks, error mapping. Use `Xunit.DependencyInjection` for test DI with a `Startup.cs` that registers Mediator and mock services.

**API integration tests:** Use `WebApplicationFactory<Program>` with SQLite in-memory replacing SQL Server. Test HTTP round-trips, status codes, Problem Details, authorization enforcement. Use `MartinCostello.Logging.XUnit.v3` for test logging. The `Microsoft.EntityFrameworkCore.Sqlite` package belongs **only** in the Api.Tests project — never add it to the Acl project (which must use `SqlServer` only).

**Do NOT** create `GlobalUsings.cs` files in test projects. Global usings come from `build/test.props`.

## Trellis Feedback

While building with Trellis, **actively track friction points, workarounds, and missing capabilities.** At the end of the project (or at any significant milestone), generate a `TRELLIS_FEEDBACK.md` file in the repository root.

This feedback helps the Trellis team identify gaps in the framework and prioritize future improvements. **Generate this file proactively** — do not wait to be asked.

### When to Record Feedback

- You had to write boilerplate that Trellis should have handled
- You worked around a missing pattern or building block
- A Trellis API was confusing or required reading source code to understand
- You wished a base class, interface, or extension method existed but it didn't
- The copilot instructions were ambiguous or missing guidance for a scenario you encountered
- An error message from Trellis was unhelpful or misleading
- You had to make an architectural decision that Trellis should have constrained
- A common .NET pattern (middleware, DI, configuration) wasn't covered by Trellis conventions

### Feedback File Format

Generate `TRELLIS_FEEDBACK.md` with this structure:

```markdown
# Trellis Feedback — {ServiceName}

> Generated by AI while building {ServiceName} on {date}.
> Trellis version: {version from Directory.Packages.props}
> AI model: {model name}

## Summary

{1-2 sentence overall assessment of the development experience with Trellis}

## Friction Points

### FP-1: {Short title}
- **Category:** Missing Building Block | Workaround Required | Ambiguous API | Missing Documentation | Error Message | Architectural Gap
- **Severity:** High (blocked progress) | Medium (slowed progress) | Low (minor inconvenience)
- **Context:** {What were you trying to do?}
- **What happened:** {What went wrong or was harder than expected?}
- **Workaround used:** {What you did instead, if anything}
- **Suggested improvement:** {What Trellis could add or change}

### FP-2: ...

## What Worked Well

{List of Trellis features that were particularly effective or easy to use. This helps the team know what NOT to change.}

## Suggested New Features

### SF-1: {Feature name}
- **Use case:** {When would this be useful?}
- **Proposed API:** {Sketch of what the API could look like}

### SF-2: ...

## Copilot Instructions Feedback

{Any sections of the copilot instructions that were unclear, missing, or led to incorrect code generation. Be specific about which section and what was confusing.}
```

### Rules

- **Be specific.** Include the exact code you wrote as a workaround. Vague feedback like "EF Core was hard" is not actionable.
- **One friction point per entry.** Don't combine unrelated issues.
- **Include severity.** This helps the Trellis team prioritize.
- **Credit what works.** The "What Worked Well" section is equally important — it prevents regressions.
- **If nothing went wrong, say so.** A feedback file with zero friction points and a strong "What Worked Well" section is valuable data.
