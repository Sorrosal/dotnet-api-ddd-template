---
description: Scaffold a complete bounded context/module with all best practices - aggregate, commands, queries, events, controller, tests
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <BoundedContext> <AggregateName>
---

# Scaffold Complete Bounded Context Module with All Best Practices

Parse `$ARGUMENTS` to extract `BoundedContext` and `AggregateName`. If missing, ask the user.

This scaffolds a **complete, production-ready bounded context** including:
- ✅ Domain: Aggregate, ID, Errors, Events
- ✅ Application: Commands, Queries, Handlers, Validators (with SharedRules)
- ✅ Infrastructure: Repository, EF Configuration
- ✅ Api: Versioned Controller
- ✅ Tests: Unit Tests, Integration Tests
- ✅ All best practices: Soft Delete, Auditing, Result<T>, ILogger, Specifications, etc.

Follow patterns in `CLAUDE.md`, `PROJECT_STRUCTURE.md`, and `BEST_PRACTICES.md`.

---

## Complete Module Scaffold Checklist

### Domain Layer
- [ ] Create {AggregateName}Id strongly typed ID
- [ ] Create {AggregateName}Errors static class
- [ ] Create {AggregateName}CreatedDomainEvent, UpdatedDomainEvent, DeletedDomainEvent
- [ ] Create {AggregateName} aggregate root extending AuditableEntity<{AggregateName}Id>
  - [ ] Private EF Core constructor
  - [ ] Create() factory → Result<{AggregateName}>
  - [ ] Update() method → Result
  - [ ] Delete() method → soft delete
  - [ ] All properties private setters
  - [ ] RaiseDomainEvent() calls in methods
- [ ] Create I{AggregateName}Repository interface extending IRepository
  - [ ] Custom query methods
  - [ ] Specification support

### Application Layer
- [ ] Create{AggregateName}Command, Handler, Validator (uses SharedRules)
- [ ] Update{AggregateName}Command, Handler, Validator
- [ ] Delete{AggregateName}Command, Handler, Validator
- [ ] Get{AggregateName}ByIdQuery, Handler, Response
- [ ] Get{AggregateName}ListQuery, Handler, Response
- [ ] {AggregateName}ByStatus Specification (for list queries)
- [ ] Domain event handlers:
  - [ ] {AggregateName}CreatedDomainEventHandler (with ILogger)
  - [ ] {AggregateName}UpdatedDomainEventHandler
  - [ ] {AggregateName}DeletedDomainEventHandler

### Infrastructure Layer
- [ ] {AggregateName}Repository implements I{AggregateName}Repository
  - [ ] Uses SpecificationEvaluator for queries
  - [ ] Logging in all methods
- [ ] {AggregateName}Configuration (IEntityTypeConfiguration)
  - [ ] Soft delete configuration
  - [ ] Auditing fields
  - [ ] Row version for concurrency
  - [ ] Global query filter for soft delete
- [ ] Update ApplicationDbContext.cs:
  - [ ] Add DbSet<{AggregateName}>
  - [ ] Register configuration
- [ ] Update DependencyInjection.cs:
  - [ ] Register I{AggregateName}Repository, {AggregateName}Repository

### Api Layer
- [ ] {AggregateName}Controller [ApiVersion("1")] in Controllers/V1/
  - [ ] POST Create {AggregateName} → 201 Created
  - [ ] PUT Update {AggregateName} → 204 NoContent
  - [ ] DELETE {AggregateName} → 204 NoContent
  - [ ] GET by ID → 200 OK or 404 NotFound
  - [ ] GET list with pagination → 200 OK
  - [ ] ILogger injected
  - [ ] Correlation ID in log context
  - [ ] Error responses with ErrorResponse model
  - [ ] XML documentation for all endpoints
  - [ ] [Authorize] on write operations

### Tests
- [ ] UnitTests/Domain/{BoundedContext}/{AggregateName}Tests.cs
  - [ ] Create_Should_Return*_When_ValidData
  - [ ] Create_Should_ReturnFailure_When_InvalidData
  - [ ] Update_Should_RaiseDomainEvent
  - [ ] Delete_Should_SetDeletedAt (soft delete)
- [ ] UnitTests/Application/{BoundedContext}/Commands/
  - [ ] Create{AggregateName}CommandHandlerTests
  - [ ] Update{AggregateName}CommandHandlerTests
  - [ ] Delete{AggregateName}CommandHandlerTests
- [ ] UnitTests/Application/{BoundedContext}/Queries/
  - [ ] Get{AggregateName}ByIdQueryHandlerTests
  - [ ] Get{AggregateName}ListQueryHandlerTests
- [ ] IntegrationTests/Api/{BoundedContext}/{AggregateName}ControllerTests.cs
  - [ ] Post_Should_Create_When_ValidRequest → 201
  - [ ] Get_Should_Return_When_Exists → 200
  - [ ] Get_Should_ReturnNotFound_When_NotFound → 404
  - [ ] Put_Should_Update_When_Exists → 204
  - [ ] Delete_Should_Delete_When_Exists → 204
  - [ ] Post_Should_ReturnBadRequest_When_Invalid → 400

### Documentation
- [ ] Update SWAGGER - XML comments on controller
- [ ] Verify all error codes documented in ErrorResponse
- [ ] Add migration: `dotnet ef migrations add Add{AggregateName}Table ...`

---

## Quick Commands to Use

After selecting this scaffold, use individual skills for each component:

```bash
# Aggregate root with all best practices
/new-aggregate {BoundedContext} {AggregateName}

# CQRS commands
/new-command {BoundedContext} Create{AggregateName}
/new-command {BoundedContext} Update{AggregateName}
/new-command {BoundedContext} Delete{AggregateName}

# CQRS queries
/new-query {BoundedContext} Get{AggregateName}ById
/new-query {BoundedContext} Get{AggregateName}List

# Domain events
/new-domain-event {BoundedContext} {AggregateName}Created {BoundedContext}
/new-domain-event {BoundedContext} {AggregateName}Updated {BoundedContext}
/new-domain-event {BoundedContext} {AggregateName}Deleted {BoundedContext}

# Tests
/new-unit-test src/Domain/{BoundedContext}/Entities/{AggregateName}.cs
/new-integration-test {BoundedContext} {AggregateName}Controller

# Or scaffold entire module at once (requires multiple file creations)
```

---

## Key Patterns Integrated

✅ **Soft Delete** - DeletedAt field, global filter, interceptor
✅ **Auditing** - CreatedBy, ModifiedBy, timestamps auto-set
✅ **Concurrency Control** - RowVersion for optimistic locking
✅ **Result<T> Pattern** - No exceptions for control flow
✅ **Strongly Typed IDs** - Type-safe identifiers
✅ **Domain Events** - Raised in aggregate, handled asynchronously
✅ **Specification Pattern** - Reusable, composable queries
✅ **Shared Validation Rules** - ValidateName(), ValidateEmail(), etc.
✅ **Structured Logging** - ILogger<T> with correlation context
✅ **Versioned API** - api/v{version:apiVersion}/ routes
✅ **Health Checks** - /health/live, /health/ready endpoints
✅ **Exception Handling** - Structured ErrorResponse with TraceId
✅ **Authorization** - [Authorize] on sensitive operations
✅ **Swagger/OpenAPI** - Auto-generated documentation

---

## Step-by-Step Implementation Order

1. **Domain First** - Aggregate, Errors, Events
2. **Application** - Commands, Queries, Validators
3. **Infrastructure** - Repository, Configuration, DI
4. **Api** - Controller, Middleware integration
5. **Tests** - Unit tests, Integration tests
6. **Database** - Migrations and setup
7. **Documentation** - Swagger, API docs

---

## Post-Creation Steps

```bash
# Create and apply migration
dotnet ef migrations add Add{AggregateName}Table \
    --project src/Infrastructure \
    --startup-project src/Api

# Run tests
dotnet test tests/UnitTests
dotnet test tests/IntegrationTests

# Verify build and formatting
dotnet build
dotnet format --verify-no-changes

# Check Swagger
open http://localhost:5000/swagger (after running the app)
```

---

## Validation Checklist

- ✅ All files use file-scoped namespaces
- ✅ Controllers inject ILogger<T>
- ✅ Handlers inject ILogger<T>
- ✅ All factory methods return Result<T>
- ✅ All domain methods return Result or Result<T>
- ✅ Repository uses Specification<T> for queries
- ✅ Soft delete queries use global filter
- ✅ Validation rules use SharedRules
- ✅ Event handlers use ILogger
- ✅ Tests use AAA pattern
- ✅ Tests verify Result success/failure branches
- ✅ Integration tests use Testcontainers
- ✅ API endpoints have XML docs
- ✅ Swagger definitions are complete

---

**Complete reference:**
- See `BEST_PRACTICES.md` for detailed patterns
- See `PROJECT_STRUCTURE.md` for file organization
- See individual `/new-*` commands for specific components

