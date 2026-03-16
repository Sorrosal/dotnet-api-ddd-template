# ⚡ Quick Start Guide

Complete step-by-step instructions to get the API running in 5 minutes.

---

## Option 1: Docker Compose (Recommended) ⚓

### Step 1: Clone the Repository
```bash
git clone https://github.com/yourusername/dotnet-api-ddd-template.git
cd dotnet-api-ddd-template
```

### Step 2: Start Services
```bash
docker compose up --build
```

**What happens:**
- PostgreSQL starts on port 5432
- .NET API starts on https://localhost:5001
- Database migrations run automatically
- Roles (Admin, User) are seeded

### Step 3: Test the API
Open in browser: https://localhost:5001/swagger

Or test via curl:
```bash
curl https://localhost:5001/health/live
```

✅ **Done!** Your API is running.

---

## Option 2: Local Development 🖥️

### Prerequisites
```bash
# Check .NET version (should be 10.0+)
dotnet --version

# Check PostgreSQL is running
psql --version

# PostgreSQL should be running (default: localhost:5432)
# Default credentials: postgres/postgres
```

### Step 1: Clone & Restore
```bash
git clone https://github.com/yourusername/dotnet-api-ddd-template.git
cd dotnet-api-ddd-template
dotnet restore
```

### Step 2: Update Database
```bash
# Create/update database with migrations
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Api
```

### Step 3: Run the API
```bash
# Option A: Simple run
dotnet run --project src/Api

# Option B: Watch mode (auto-reload on changes)
dotnet watch run --project src/Api
```

API starts on:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: https://localhost:5001/swagger

### Step 4: Test
```bash
curl http://localhost:5000/health/live
```

✅ **Running!**

---

## First API Test 🧪

### 1. Register a User
```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePass123",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

Response:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### 2. Login
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePass123"
  }'
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "AbCdEfGhIjKlMnOpQrStUvWxYz...",
  "accessTokenExpiresAt": "2026-03-16T10:30:00Z",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john@example.com"
}
```

**Save the accessToken!** You'll need it for authenticated requests.

### 3. Create a Customer
```bash
# Replace YOUR_TOKEN with the accessToken from login
curl -X POST http://localhost:5000/api/v1/customers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "Acme Corporation",
    "email": "contact@acme.com",
    "phoneNumber": "+1-555-0123",
    "address": "123 Main Street",
    "city": "New York",
    "country": "USA"
  }'
```

Response:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Acme Corporation",
  "email": "contact@acme.com",
  "phoneNumber": "+1-555-0123",
  "address": "123 Main Street",
  "city": "New York",
  "country": "USA",
  "createdBy": "550e8400-e29b-41d4-a716-446655440000",
  "createdAtUtc": "2026-03-16T10:00:00Z",
  "modifiedBy": null,
  "modifiedAtUtc": null,
  "isDeleted": false
}
```

### 4. Get All Customers
```bash
curl -X GET http://localhost:5000/api/v1/customers
```

### 5. Update a Customer
```bash
curl -X PUT http://localhost:5000/api/v1/customers/550e8400-e29b-41d4-a716-446655440001 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "Acme Corporation Updated",
    "email": "newemail@acme.com",
    "phoneNumber": "+1-555-9999",
    "address": "456 Oak Avenue",
    "city": "Los Angeles",
    "country": "USA"
  }'
```

### 6. Delete a Customer
```bash
curl -X DELETE http://localhost:5000/api/v1/customers/550e8400-e29b-41d4-a716-446655440001 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 7. Refresh Token
```bash
curl -X POST http://localhost:5000/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

### 8. Logout
```bash
curl -X POST http://localhost:5000/api/v1/auth/logout \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

---

## Run Tests 🧬

### Unit Tests Only
```bash
dotnet test tests/UnitTests
```

### Integration Tests Only
```bash
dotnet test tests/IntegrationTests
```

### All Tests
```bash
dotnet test
```

### With Code Coverage
```bash
dotnet test /p:CollectCoverage=true
```

---

## Development Workflow 💻

### 1. Create a New Feature (Example: Add Products)

**Step 1: Create domain entity**
```bash
# Create file: src/Domain/Products/Entities/Product.cs
```

**Step 2: Create repository interface**
```bash
# Create file: src/Domain/Products/Repositories/IProductRepository.cs
```

**Step 3: Create create command**
```bash
# Create files:
# src/Application/Features/Products/Commands/Create/CreateProductCommand.cs
# src/Application/Features/Products/Commands/Create/CreateProductCommandHandler.cs
# src/Application/Features/Products/Commands/Create/CreateProductCommandValidator.cs
```

**Step 4: Create query**
```bash
# Create files:
# src/Application/Features/Products/Queries/GetProductById/GetProductByIdQuery.cs
# src/Application/Features/Products/Queries/GetProductById/GetProductByIdQueryHandler.cs
# src/Application/Features/Products/Queries/GetProductById/GetProductByIdResponse.cs
```

**Step 5: Create API controller**
```bash
# Create file: src/Api/Controllers/V1/ProductsController.cs
```

**Step 6: Create tests**
```bash
# Create files:
# tests/UnitTests/Domain/Products/ProductTests.cs
# tests/IntegrationTests/Api/Products/ProductsControllerTests.cs
```

### 2. Format Code
```bash
dotnet format
```

### 3. Run Tests
```bash
dotnet test
```

### 4. Commit with Conventional Commits
```bash
git add .
git commit -m "feat: add products feature"
# or
git commit -m "fix: resolve customer validation bug"
# or
git commit -m "refactor: simplify auth service"
```

---

## Common Tasks 📋

### Add a Database Migration
```bash
dotnet ef migrations add AddProductsTable \
  --project src/Infrastructure \
  --startup-project src/Api
```

### Revert Database Migration
```bash
dotnet ef database update PreviousMigrationName \
  --project src/Infrastructure \
  --startup-project src/Api
```

### Remove Last Migration
```bash
dotnet ef migrations remove \
  --project src/Infrastructure \
  --startup-project src/Api
```

### Check Database Connection
```bash
# From Project root, ensure appsettings.json has correct ConnectionString
cat src/Api/appsettings.json
```

### View All Database Tables
```bash
psql -h localhost -U postgres -d dotnet-api-ddd -c "\dt"
```

### Clear Database & Re-migrate
```bash
# Drop database
dropdb -h localhost -U postgres dotnet-api-ddd

# Recreate with migrations
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Api
```

---

## Troubleshooting 🔧

### "Cannot connect to PostgreSQL"
```bash
# Check if PostgreSQL is running
psql --version

# If using Docker:
docker compose logs postgres

# Check connection string in appsettings.json
```

### "Migrations not found"
```bash
# Ensure correct paths
dotnet ef migrations list \
  --project src/Infrastructure \
  --startup-project src/Api
```

### "Port 5001 already in use"
```bash
# Find process using port
lsof -i :5001

# Kill process
kill -9 <PID>

# Or change port in launchSettings.json
```

### "JWT Token Expired"
```bash
# Use refresh endpoint to get new token
POST /api/v1/auth/refresh
```

### Tests Fail - "Database Connection Error"
```bash
# Ensure PostgreSQL is running
# Check docker compose logs if using containers
docker compose logs postgres

# Or restart services
docker compose restart
```

---

## Project Structure Quick Reference 🗂️

```
src/Domain/               ← Business logic, never depends on anything
src/Application/          ← CQRS commands/queries, depends on Domain
src/Infrastructure/       ← Database, Identity, Email, depends on Application+Domain
src/Api/                  ← REST endpoints, depends on Infrastructure+Application

tests/UnitTests/          ← Test Domain + Application layer
tests/IntegrationTests/   ← Test API endpoints with real database
```

---

## Performance Tips ⚡

### Enable Logging Details
In `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

### Monitor Database Queries
EF Core logs all SQL executed when `Microsoft.EntityFrameworkCore` log level is `Debug`.

Check Serilog output in console.

---

## Security Checklist 🔒

Before deploying to production:

- [ ] Change JWT Secret to a long, random string (minimum 256 bits)
- [ ] Update password policy if needed (min length, complexity)
- [ ] Enable HTTPS (already enabled in development)
- [ ] Set CORS properly (don't use AllowAnyOrigin)
- [ ] Rotate secrets regularly
- [ ] Use environment variables for sensitive config
- [ ] Enable rate limiting on auth endpoints
- [ ] Set up monitoring/alerting for failed logins
- [ ] Review CLAUDE.md for architecture rules

---

## Next Steps 🎯

1. **Read [CLAUDE.md](CLAUDE.md)** - Understand architecture rules
2. **Explore [README.md](README.md)** - Full API documentation
3. **Check database** - Login to PostgreSQL, explore schema
4. **Review code** - Look at Customers feature as reference
5. **Create your feature** - Follow the same patterns
6. **Write tests** - Unit + Integration tests
7. **Deploy** - Use your favorite hosting (Azure, AWS, etc.)

---

## Quick Command Reference 📖

| Command | Purpose |
|---------|---------|
| `docker compose up --build` | Start entire stack |
| `dotnet run --project src/Api` | Run API locally |
| `dotnet test` | Run all tests |
| `dotnet format` | Format code |
| `dotnet ef migrations add Name --project src/Infrastructure --startup-project src/Api` | Create migration |
| `dotnet ef database update --project src/Infrastructure --startup-project src/Api` | Apply migrations |
| `dotnet watch run --project src/Api` | Run with auto-reload |

---

**You're all set! 🎉 Start building amazing features!**

For detailed architecture info, see [CLAUDE.md](CLAUDE.md)
