---
name: Architecture Structure
description: Layer organization, all 14 best practices, ILogger, and Result Pattern
type: project
---

## Complete Layer Organization (English)

### Domain Layer (src/Domain)
Enterprise business rules, zero external dependencies

**Components:**
- Entities (Aggregates), Value Objects, Domain Events
- Repositories (interfaces only), Errors, Enums, Specifications
- Base classes: BaseEntity (soft delete + row version), AuditableEntity, AggregateRoot
- Strongly typed IDs: Entity : AggregateRoot<EntityId>

**Restrictions:**
- ✅ Zero NuGet dependencies (except primitives)
- ✅ Pure business logic only
- ✅ Result<T> for domain errors
- ❌ No exceptions for control flow
- ❌ No database access

---

### Application Layer (src/Application)
Use cases (CQRS), orchestration, validation, specifications

**Components:**
- Commands (state changes), Queries (reads), Handlers
- Validators (FluentValidation with SharedRules)
- DTOs, Domain Event handlers
- Specifications (reusable query logic)
- Pipeline Behaviors: Validation → Logging → UnitOfWork
- Exceptions: ApplicationException and subtypes

**Restrictions:**
- ✅ Depends on Domain only
- ✅ CQRS separated (Commands vs Queries)
- ✅ Validation in Validators, not handlers
- ❌ No business logic
- ❌ No Infrastructure injection (only interfaces)

**Logging:**
- ILogger<HandlerName> injected in all handlers
- logger.LogInformation() for normal flow
- logger.LogWarning() for business errors
- logger.LogError() for exceptions

---

### Infrastructure Layer (src/Infrastructure)
Technical implementations, persistence, external services

**Components:**
- DbContext with global query filters
- Repositories with Specification support
- EF Core Interceptors:
  - DomainEventDispatcherInterceptor (dispatches domain events)
  - AuditableInterceptor (sets CreatedBy, ModifiedBy, timestamps)
  - SoftDeleteInterceptor (sets DeletedAt instead of DELETE)
- UnitOfWork (transaction management with concurrency handling)
- Configuration classes (DatabaseSettings, JwtSettings, etc.)
- Services: CurrentUserService, RequestContextService, etc.

**Restrictions:**
- ✅ Implements Application/Domain interfaces
- ✅ Database, external APIs, file systems
- ❌ No reference from Domain/Application

**Logging:**
- ILogger injected in all services and interceptors
- Used for persistence operations, migrations, concurrency events

---

### Api Layer (src/Api)
HTTP/REST, authentication, versioning, documentation

**Components:**
- Controllers (V1 versioned, minimal logic)
- Middleware:
  - CorrelationIdMiddleware (adds X-Correlation-ID)
  - RequestResponseLoggingMiddleware (audits all requests/responses)
  - GlobalExceptionHandlerMiddleware (handles exceptions)
- Extensions: OpenApi, Authorization, HealthCheck, DependencyInjection
- Filters: SwaggerOperationFilter, SwaggerSchemaFilter
- Models: ErrorResponse, DTOs

**Restrictions:**
- ✅ Controllers very thin: request → MediatR → response
- ✅ Mandatory versioning [ApiVersion("1")]
- ✅ Routes: api/v{version:apiVersion}/...
- ❌ No business logic
- ❌ No domain knowledge

**Logging:**
- ILogger<ControllerName> in all controllers
- ILogger used in all middleware
- CorrelationId included in all logs via BeginScope()

---

## All 14 Best Practices Overview

### Critical Tier (🔴 Must Implement)

1. **Specification Pattern** (Domain.Common.Specifications)
   - ISpecification<T>, Specification<T>
   - SpecificationEvaluator<T> in Infrastructure
   - Usage: GetBySpecificationAsync(spec) in repositories

2. **Correlation ID** (Api.Middleware, Infrastructure.Services)
   - CorrelationIdMiddleware + IRequestContext
   - X-Correlation-ID header
   - Propagated to all logs via BeginScope()

3. **Soft Delete** (Domain.BaseEntity, Infrastructure.Interceptors)
   - DateTime? DeletedAt field
   - SoftDeleteInterceptor sets it instead of DELETE
   - Global query filter excludes soft-deleted entities

4. **Entity Auditing** (Domain.AuditableEntity, Infrastructure.Interceptors)
   - Guid CreatedBy, DateTime CreatedAtUtc
   - Guid? ModifiedBy, DateTime? ModifiedAtUtc
   - AuditableInterceptor auto-sets on Add/Modify

5. **Swagger/OpenAPI** (Api.Extensions.OpenApiExtensions)
   - /swagger endpoint
   - Automatic XML documentation
   - JWT authentication scheme
   - Correlation ID header parameter

6. **Authorization** (Infrastructure.CurrentUserService, Api.Extensions.AuthorizationExtensions)
   - ICurrentUser interface
   - JWT bearer token authentication
   - Policies: AdminOnly, OrderManagement, ProductManagement, ReadOnly
   - [Authorize(Policy = "AdminOnly")] on controllers/actions

### Important Tier (🟡 Highly Recommended)

7. **Optimistic Concurrency** (Domain.BaseEntity)
   - [Timestamp] byte[] RowVersion
   - EF Core auto-increments on modifications
   - DbUpdateConcurrencyException caught in UnitOfWorkBehavior

8. **Exception Handling** (Application.Common.Exceptions, Api.Middleware)
   - ApplicationException (base), NotFoundException, ValidationException, etc.
   - GlobalExceptionHandlerMiddleware returns ErrorResponse
   - Structured error responses with Code, Message, TraceId

9. **Health Checks** (Api.Extensions.HealthCheckExtensions)
   - /health/live - liveness check (is API running?)
   - /health/ready - readiness check (are all dependencies ready?)
   - /health - detailed status with all checks

10. **Request/Response Logging** (Api.Middleware.RequestResponseLoggingMiddleware)
    - Logs full request: Method, Path, Headers, Body
    - Logs full response: Status, Duration, Body
    - Truncates body > 4096 chars
    - Includes CorrelationId in all logs

11. **Configuration Management** (Infrastructure.Configuration)
    - DatabaseSettings: ConnectionString, Timeouts, Retries
    - JwtSettings: Secret, Issuer, Audience, ExpirationMinutes
    - LoggingSettings: LogLevel, Enabled features
    - AppSettings: Aggregates all above
    - appsettings.json (committed), appsettings.*.json (git-ignored for secrets)

12. **Transactional Consistency** (Infrastructure.Persistence.UnitOfWork)
    - IUnitOfWork.ExecuteTransactionAsync<T>(action, isolationLevel, ct)
    - Explicit transaction control
    - Handles DbUpdateConcurrencyException
    - Logging of transaction lifecycle

### Additional Tier (🟢 Code Quality)

13. **Validation Rules Reusability** (Application.Common.Validators.SharedRules)
    - Extension methods on IRuleBuilder<T, TProperty>
    - ValidateName(), ValidateEmail(), ValidateDescription()
    - ValidateQuantity(), ValidatePrice(), ValidateId()
    - Reused via .ValidateName() in any validator

14. **Architectural Testing** (tests.ArchitectureTests)
    - LayerDependencyTests: Enforce boundaries
    - NamingConventionTests: Verify patterns
    - ProjectReferenceTests: Prevent wrong references

---

## Result<T> Pattern Throughout

**Core Concept:**
```
Instead of: try/catch exceptions
Use: Result<T> with explicit success/failure states
```

**Structure:**
- bool IsSuccess
- T? Value (only when IsSuccess)
- Error Error (only when !IsSuccess)
  - string Code (e.g., "Order.InvalidName")
  - string Message (e.g., "Order name cannot be empty")

**Usage:**
- Domain: Order.Create() returns Result<Order>
- Application: CreateOrderCommandHandler returns Result<OrderId>
- Api: Check result.IsSuccess ? Ok(value) : BadRequest(error)

**Handling:**
```csharp
result.Match(
    onSuccess: order => /* handle success */,
    onFailure: error => /* handle error */
)
```

---

## ILogger Usage Pattern

**Injection:** ILogger<ClassName> logger (constructor injection)

**Levels:**
- LogInformation() - normal operations ("Creating order")
- LogWarning() - business errors ("Order not found")
- LogError(ex, "message") - system failures
- LogDebug() - detailed diagnostics

**Structured Logging:**
```csharp
logger.LogInformation(
    "Creating order: {Name}, CorrelationId: {CorrelationId}",
    request.Name,
    requestContext.CorrelationId);
```

**Scope for CorrelationId:**
```csharp
using (logger.BeginScope(new Dictionary<string, object>
{
    { "CorrelationId", correlationId }
}))
{
    // All logs here include CorrelationId
}
```

---

## Dependency Flow

```
Domain (↑ from nothing)
  ↑
Application (→ Domain)
  ↑
Infrastructure (→ Application + Domain)
  ↑
Api (→ Infrastructure for DI only)
```

**Violations caught by ArchitectureTests**

---

## Key Naming Conventions

Commands: `{Verb}{Noun}Command` (CreateOrderCommand)
Queries: `Get{Noun}[List]Query` (GetOrderByIdQuery)
Handlers: `{Command|Query}Handler`
Validators: `{Command|Query}Validator`
Domain Events: `{Noun}{PastVerb}DomainEvent` (OrderCreatedDomainEvent)
Strongly Typed IDs: `{Entity}Id` (OrderId)
Tests: `{Method}_Should_{Expected}_When_{Condition}`

---

## Request Processing Flow (Complete)

```
1. HTTP Request
   ↓ POST /api/v1/orders

2. CorrelationIdMiddleware
   - Adds/extracts X-Correlation-ID header
   - Stores in context.Items["CorrelationId"]
   - logger.BeginScope(correlationId)
   ↓

3. RequestResponseLoggingMiddleware
   - Logs request details
   - Logs response (after execution)
   ↓

4. GlobalExceptionHandlerMiddleware (wraps next)
   - Catches all unhandled exceptions
   - Returns ErrorResponse
   ↓

5. OrdersController.Create()
   - Logger.LogInformation("Creating order")
   - Parse request → CreateOrderCommand
   ↓

6. MediatR Pipeline
   - ValidationBehavior: CreateOrderCommandValidator
   - LoggingBehavior: logs start/end
   ↓

7. CreateOrderCommandHandler
   - Logger.LogInformation("Processing")
   - IOrderRepository, IUnitOfWork injected
   - Order.Create() → Result<Order>
   - If failure: Logger.LogWarning() + return failure
   ↓

8. UnitOfWork.SaveChangesAsync()
   - EF SaveChanges triggers interceptors:
     - DomainEventDispatcherInterceptor: detects OrderCreatedDomainEvent
     - AuditableInterceptor: sets CreatedBy, CreatedAtUtc
     - SoftDeleteInterceptor: handles any deletes
   - DbUpdateConcurrencyException caught + logged
   ↓

9. MediatR Dispatch Domain Events
   - OrderCreatedDomainEventHandler reacts
   - Logger.LogInformation() in handler
   ↓

10. Return to Controller
    - Result<OrderId>.IsSuccess check
    - 201 Created with Location header
    ↓

11. Response logged by RequestResponseLoggingMiddleware
    - Status, duration, body
```

---

**This architecture ensures:**
- ✅ Clear separation of concerns
- ✅ Enterprise-grade observability (logging, tracing)
- ✅ Production-ready error handling
- ✅ Auditable changes (who/when/what)
- ✅ Data integrity (concurrency, transactions)
- ✅ Type safety (strongly typed IDs)
- ✅ Maintainability (clear patterns)
