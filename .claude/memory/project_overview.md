---
name: Project Overview
description: Complete .NET 10 DDD/CQRS template with 14 enterprise best practices
type: project
---

## Project: DotnetApiDddTemplate

A complete, production-ready .NET 10 Web API template following Domain-Driven Design (DDD), Clean Architecture, CQRS pattern, and 14 enterprise best practices.

**Complete Stack:**
- **.NET 10 LTS** - Latest long-term support framework
- **PostgreSQL** - Production database
- **Entity Framework Core 10** - ORM with advanced interceptors
- **MediatR** - CQRS implementation
- **FluentValidation** - Declarative validation rules
- **ILogger** (Microsoft.Extensions.Logging) - Built-in structured logging
- **xUnit** - Unit testing framework
- **Testcontainers** - Integration test infrastructure
- **Swagger/OpenAPI** - Interactive API documentation

## 14 Implemented Best Practices

### Critical Practices (🔴)
1. ✅ **Specification Pattern** - Composable, reusable query logic
2. ✅ **Correlation ID/Tracing** - Distributed request tracking
3. ✅ **Soft Delete** - Preserve historical data
4. ✅ **Entity Auditing** - Who, what, when tracking
5. ✅ **Swagger/OpenAPI** - Auto-generated API documentation
6. ✅ **Authorization Policies** - Role/claim-based access control

### Important Practices (🟡)
7. ✅ **Optimistic Concurrency** - Race condition prevention
8. ✅ **Exception Handling** - Structured error responses
9. ✅ **Health Checks** - Application monitoring
10. ✅ **Request/Response Logging** - Complete audit trail
11. ✅ **Configuration Management** - Secure setup
12. ✅ **Transactional Consistency** - Explicit transaction control

### Additional Practices (🟢)
13. ✅ **Validation Rules Reusability** - Shared validation logic
14. ✅ **Architectural Testing** - Enforce boundaries

## Key Architectural Features

- **Zero-Business-Logic Database** - All logic in Domain/Application layers
- **Result<T> Pattern** - No exceptions for control flow
- **Strongly Typed IDs** - Type-safe aggregate identifiers
- **Domain Events** - Async event handling with interceptors
- **Repository Pattern** - Clean data access abstractions
- **Unit of Work** - Transaction management with rollback support
- **Global Query Filters** - Automatic soft-delete filtering
- **Interceptors** - Domain events, auditing, soft deletes
- **Middleware Pipeline** - Exception handling, logging, correlation
- **Health Monitoring** - K8s/container-ready health endpoints

## Documentation

- **CLAUDE.md** - Architecture rules and conventions
- **PROJECT_STRUCTURE.md** - Complete folder organization
- **BEST_PRACTICES.md** - Full implementation guide with code examples (50+ pages)

## Why This Template?

**Problem:** Most projects lack proper architecture from day one, leading to technical debt.

**Solution:** This template enforces architectural best practices and enterprise patterns, making it:
- ✅ Production-ready out of the box
- ✅ Scalable from day one
- ✅ Maintainable long-term
- ✅ Team-friendly (clear patterns for new developers)
- ✅ Audit-ready (complete tracking and logging)
- ✅ Cloud-native (health checks, correlation IDs, monitoring)

## Quick Start

```bash
# Build
dotnet build

# Run
dotnet run --project src/Api

# Test
dotnet test tests/UnitTests
dotnet test tests/IntegrationTests
dotnet test tests/ArchitectureTests

# Database
docker compose up --build
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api
```

## Folder Structure

```
src/
  Api/            - HTTP layer, Swagger, Auth, Health checks
  Application/    - CQRS, Specifications, Validators, DTOs
  Domain/         - Entities, Value Objects, Events, Aggregates
  Infrastructure/ - Database, Repositories, Interceptors, Config

tests/
  UnitTests/        - Domain + Application tests
  IntegrationTests/ - API + Database tests
  ArchitectureTests/- Layer boundary validation
```

**All documentation, code organization, and patterns follow enterprise best practices.**
