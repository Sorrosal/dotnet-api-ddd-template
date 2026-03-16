# 🏗️ .NET 10 Domain-Driven Design API Template

A production-ready REST API built with **Domain-Driven Design (DDD)**, **Clean Architecture**, **CQRS**, and **Result Pattern**. Features comprehensive implementations of 14+ enterprise best practices.

## 📋 Quick Features

✅ **Authentication & Authorization**
- ASP.NET Identity with JWT Bearer tokens
- Role-based access control (Admin, User)
- Refresh token rotation with automatic revocation
- Secure password hashing

✅ **Enterprise Patterns**
- Domain-Driven Design (bounded contexts, aggregates, value objects)
- CQRS with MediatR
- Specification Pattern for queries
- Result<T> pattern (no exceptions for control flow)
- Soft delete & entity auditing
- Optimistic concurrency control

✅ **Code Quality & Testing**
- Unit & Integration tests with xUnit
- Testcontainers for PostgreSQL
- Swagger/OpenAPI documentation
- Code coverage tracking
- EditorConfig formatting rules

✅ **Production Ready**
- Structured logging with Serilog
- Health check endpoints (/health/live, /health/ready)
- API versioning (v1, v2, ...)
- Global exception handling
- Database migrations support

---

## 🚀 Quick Start

### 1. Prerequisites
- .NET 10 SDK
- PostgreSQL 14+ (or Docker)
- Docker & Docker Compose (optional, but recommended)

### 2. Clone & Restore
```bash
git clone https://github.com/yourusername/dotnet-api-ddd-template.git
cd dotnet-api-ddd-template
dotnet restore
```

### 3. Run with Docker Compose
```bash
docker compose up --build
```

Starts:
- PostgreSQL on port 5432 (user: postgres, pass: postgres)
- API on https://localhost:5001

### 4. Access the API
- **Swagger UI**: https://localhost:5001/swagger
- **Health Check**: https://localhost:5001/health/live

### 5. Test Authentication
```bash
# Register
curl -X POST https://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123",
    "firstName": "John",
    "lastName": "Doe"
  }'

# Login
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123"
  }'

# Use the returned accessToken in Authorization header:
# Authorization: Bearer <accessToken>
```

---

## 📚 API Documentation

### Authentication Endpoints

#### Register User
```
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "MinimumLength8",
  "firstName": "John",
  "lastName": "Doe"
}

Response 201:
{
  "userId": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### Login
```
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "MinimumLength8"
}

Response 200:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "AbCdEfGhIjKlMnOpQrStUvWxYz...",
  "accessTokenExpiresAt": "2026-03-16T10:45:00Z",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com"
}
```

#### Refresh Token
```
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "AbCdEfGhIjKlMnOpQrStUvWxYz..."
}

Response 200:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "NewRefreshToken...",
  "accessTokenExpiresAt": "2026-03-16T10:45:00Z",
  "userId": "...",
  "email": "..."
}
```

#### Logout
```
POST /api/v1/auth/logout
Content-Type: application/json

{
  "refreshToken": "AbCdEfGhIjKlMnOpQrStUvWxYz..."
}

Response 204 No Content
```

### Customers Endpoints

#### Create Customer (Requires JWT)
```
POST /api/v1/customers
Content-Type: application/json
Authorization: Bearer <accessToken>

{
  "name": "Acme Corp",
  "email": "contact@acme.com",
  "phoneNumber": "+1234567890",
  "address": "123 Main St",
  "city": "New York",
  "country": "USA"
}

Response 201:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Acme Corp",
  "email": "contact@acme.com",
  "phoneNumber": "+1234567890",
  "address": "123 Main St",
  "city": "New York",
  "country": "USA"
}
```

#### Get All Customers (Paginated)
```
GET /api/v1/customers?page=1&pageSize=10&search=Acme

Response 200:
{
  "items": [...],
  "currentPage": 1,
  "totalPages": 5,
  "totalCount": 50,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

#### Get Customer by ID
```
GET /api/v1/customers/{customerId}

Response 200:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Acme Corp",
  ...
}
```

#### Update Customer (Requires JWT)
```
PUT /api/v1/customers/{customerId}
Content-Type: application/json
Authorization: Bearer <accessToken>

{
  "name": "Acme Corp Updated",
  "email": "newemail@acme.com",
  ...
}

Response 200:
{
  ...updated customer...
}
```

#### Delete Customer (Soft Delete, Requires JWT)
```
DELETE /api/v1/customers/{customerId}
Authorization: Bearer <accessToken>

Response 204 No Content
```

### Health Checks

#### Liveness Probe
```
GET /health/live

Response 200:
Healthy
```

#### Readiness Probe
```
GET /health/ready

Response 200:
Healthy
```

---

## 🏛️ Architecture Overview

### Layer Structure
```
┌─────────────────────────────────────────┐
│           API Layer (Controllers)        │ ← REST endpoints, versioning
├─────────────────────────────────────────┤
│      Application Layer (CQRS)            │ ← Commands, Queries, Validators
├─────────────────────────────────────────┤
│       Domain Layer (Pure Logic)          │ ← Entities, Value Objects, Events
├─────────────────────────────────────────┤
│    Infrastructure Layer (Frameworks)     │ ← Database, Identity, DI
└─────────────────────────────────────────┘
```

### Directory Structure
```
src/
├── Api/                           # Presentation layer
│   ├── Controllers/V1/            # API endpoints
│   │   ├── AuthController.cs      # Authentication endpoints
│   │   └── CustomersController.cs # Customer CRUD endpoints
│   ├── Requests/                  # Request DTOs
│   ├── Models/                    # API response models
│   ├── Program.cs                 # Application startup
│   ├── appsettings.json           # Configuration
│   └── GlobalUsings.cs            # Global using statements
│
├── Application/                   # Application layer (CQRS)
│   ├── Features/                  # Feature modules
│   │   ├── Auth/                  # Authentication feature
│   │   │   ├── Commands/          # Register, Login, RefreshToken, Logout
│   │   │   ├── Interfaces/        # IAuthService
│   │   │   ├── Models/            # AuthResponse DTO
│   │   │   └── Errors/            # Auth-specific errors
│   │   └── Customers/             # Customer feature
│   │       ├── Commands/          # Create, Update, Delete
│   │       ├── Queries/           # GetById, GetList
│   │       └── Events/            # Domain event handlers
│   ├── Common/                    # Shared application infrastructure
│   │   ├── Behaviors/             # MediatR pipeline behaviors
│   │   ├── Interfaces/            # IRepository, IUnitOfWork, ICurrentUser
│   │   └── Models/                # Result<T>, Error, PagedList
│   ├── Extensions/                # DI extension methods
│   └── GlobalUsings.cs
│
├── Domain/                        # Domain layer (pure business logic)
│   ├── Common/                    # Base classes
│   │   ├── BaseEntity.cs          # Entity base with domain events
│   │   ├── AuditableEntity.cs     # Soft delete & auditing
│   │   ├── ValueObject.cs         # Value object base
│   │   └── Models/                # Result<T>, Error
│   ├── Customers/                 # Bounded context
│   │   ├── Entities/              # Aggregate roots & entities
│   │   ├── ValueObjects/          # Strongly typed IDs
│   │   ├── Events/                # Domain events
│   │   ├── Errors/                # Domain-specific errors
│   │   └── Repositories/          # Repository interfaces
│   └── GlobalUsings.cs
│
└── Infrastructure/                # Infrastructure layer
    ├── Persistence/               # Database
    │   ├── ApplicationDbContext.cs # EF Core context
    │   ├── UnitOfWork.cs          # Transaction management
    │   ├── Interceptors/          # EF Core interceptors
    │   ├── Configurations/        # Entity mapping (Fluent API)
    │   ├── Repositories/          # Repository implementations
    │   └── Migrations/            # EF Core migrations
    ├── Identity/                  # Authentication
    │   ├── ApplicationUser.cs      # Identity user extension
    │   ├── ApplicationRefreshToken.cs
    │   ├── JwtOptions.cs          # JWT configuration
    │   └── Services/AuthService.cs # Authentication service
    ├── Extensions/                # DI registration
    └── GlobalUsings.cs

tests/
├── UnitTests/                     # Unit test suite
│   ├── Domain/                    # Domain logic tests
│   ├── Application/               # CQRS handler tests
│   └── Common/                    # Shared test utilities
│
└── IntegrationTests/              # Integration test suite
    ├── Api/                       # API endpoint tests
    ├── Infrastructure/            # Database tests
    └── Common/                    # Test fixtures
```

---

## 🔧 Development Guide

### Running Tests
```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test tests/UnitTests

# Run only integration tests
dotnet test tests/IntegrationTests

# With code coverage
dotnet test /p:CollectCoverage=true
```

### Code Formatting
```bash
# Check formatting
dotnet format --verify-no-changes

# Apply formatting
dotnet format
```

### Database Migrations
```bash
# Create a new migration
dotnet ef migrations add MigrationName \
  --project src/Infrastructure \
  --startup-project src/Api

# Apply migrations
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Api

# Revert last migration
dotnet ef migrations remove \
  --project src/Infrastructure \
  --startup-project src/Api
```

### Watch Mode (Auto-reload on changes)
```bash
dotnet watch run --project src/Api
```

---

## 📋 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dotnet-api-ddd;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-very-long-secret-key-at-least-256-bits",
    "Issuer": "your-app",
    "Audience": "your-app-users",
    "ExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Environment Variables
For production, override via environment variables:
```bash
# Database
ConnectionStrings__DefaultConnection=Host=prod-db;...

# JWT
Jwt__Secret=<very-long-secret-key>
Jwt__Issuer=your-app
Jwt__Audience=your-app-users

# Logging
Logging__LogLevel__Default=Warning
```

---

## 🔐 Security Best Practices

### Password Policy
- Minimum 8 characters
- At least one digit required
- Email must be unique per user

### JWT Configuration
- Algorithm: HS256 (HMAC-SHA256)
- Access token: 15 minutes expiry
- Refresh token: 7 days expiry
- Minimum secret length: 256 bits

### Recommendations
1. **Use HTTPS in production** - All sensitive data must be encrypted in transit
2. **Rotate secrets regularly** - Update JWT secret quarterly
3. **Store secrets securely** - Use Azure Key Vault, AWS Secrets Manager, or equivalent
4. **Enable CORS carefully** - Don't allow AllowAnyOrigin in production
5. **Implement rate limiting** - Add throttling on authentication endpoints
6. **Monitor failed logins** - Alert on suspicious activity

---

## 📊 Project Statistics

| Metric | Value |
|--------|-------|
| .NET Version | 10 (LTS) |
| Database | PostgreSQL 14+ |
| Solution Projects | 6 |
| Bounded Contexts | 2 (Auth, Customers) |
| Implemented Best Practices | 14+ |
| Test Coverage | Unit + Integration |
| API Endpoints | 8 |
| Authentication Methods | JWT Bearer + ASP.NET Identity |

---

## 🔗 Key Dependencies

### Core Framework
- **MediatR** 12.4.1 - CQRS command/query dispatcher
- **Entity Framework Core** 10.0.0 - ORM with PostgreSQL provider
- **FluentValidation** 11.9.2 - Validation framework
- **Serilog** 10.0.0 - Structured logging

### Authentication
- **ASP.NET Identity** 10.0.0 - User/role management
- **System.IdentityModel.Tokens.Jwt** 8.9.0 - JWT handling
- **Microsoft.AspNetCore.Authentication.JwtBearer** 10.0.0 - JWT middleware

### API & Documentation
- **Swashbuckle.AspNetCore** 6.8.0 - Swagger/OpenAPI
- **Asp.Versioning.Mvc** 8.1.0 - API versioning

### Testing
- **xUnit** 2.9.3 - Test framework
- **NSubstitute** 5.1.0 - Mocking
- **FluentAssertions** 7.0.0 - Assertions
- **Testcontainers** 4.0.1 - Container testing

---

## 📖 Documentation Files

- **[CLAUDE.md](CLAUDE.md)** - Detailed architecture rules and conventions
- **[README.md](README.md)** (this file) - Quick start and API reference
- **[QUICKSTART.md](QUICKSTART.md)** - Step-by-step getting started guide

---

## 🤝 Contributing

1. Follow the architecture rules in [CLAUDE.md](CLAUDE.md)
2. Use Conventional Commits for commit messages
3. Run tests before pushing: `dotnet test`
4. Format code: `dotnet format`
5. Create a pull request with a clear description

---

## 📝 License

MIT License - See LICENSE file for details

---

## 🙋 Support

- **Issues**: Open an issue on GitHub
- **Discussions**: Use GitHub Discussions for questions
- **Documentation**: See [CLAUDE.md](CLAUDE.md) for architecture details

---

## ✨ Key Features at a Glance

### 🎯 Domain-Driven Design
- Clear separation of concerns across layers
- Bounded contexts (Auth, Customers)
- Aggregates with domain events
- Value objects with structural equality

### 🔄 CQRS Pattern
- Commands for state changes (Create, Update, Delete)
- Queries for reads (GetById, GetList)
- Separate command and query handlers
- Validators for every command/query

### 🛡️ Authentication & Authorization
- ASP.NET Identity for user/role management
- JWT Bearer tokens for stateless auth
- Refresh token rotation with automatic revocation
- Role-based access control (RBAC)

### 🔍 Error Handling
- Result<T> pattern (no thrown exceptions for business logic)
- Domain-specific error types
- Global exception handling middleware
- Consistent error response format

### 📊 Data Management
- Soft delete with IsDeleted flag
- Entity auditing (CreatedBy, ModifiedBy, timestamps)
- Optimistic concurrency control
- Specification Pattern for complex queries

### ✅ Code Quality
- Unit & Integration test suites
- Structured logging with Serilog
- EditorConfig rules for consistency
- Health check endpoints

---

**Happy coding! 🚀**
