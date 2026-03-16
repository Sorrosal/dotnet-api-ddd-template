# 📚 Documentation Index

Complete guide to all project documentation and where to find what you need.

---

## 🚀 Getting Started

### First Time Here?
1. Start with **[README.md](README.md)** (5 min read)
   - Quick features overview
   - Quick start instructions
   - API endpoint reference

2. Then **[QUICKSTART.md](QUICKSTART.md)** (10 min read)
   - Step-by-step getting started
   - Docker or local development setup
   - First API tests with curl

3. Finally **[ARCHITECTURE.md](ARCHITECTURE.md)** (20 min read)
   - Deep understanding of DDD & CQRS
   - Code examples for each pattern
   - Best practices & anti-patterns

---

## 📖 Documentation by Role

### 👤 I'm a Developer Starting a New Feature

1. **Read**: [CLAUDE.md](CLAUDE.md) - Architecture rules & naming conventions
2. **Reference**: [ARCHITECTURE.md](ARCHITECTURE.md) - DDD, CQRS, Result pattern details
3. **Copy**: Look at `Customers` feature as a template
4. **Follow**: The CQRS pattern (Command/Query → Handler → Validator → Repository)
5. **Test**: Write unit & integration tests

**Key Files to Study**:
- `src/Domain/Customers/Entities/Customer.cs` - Aggregate root example
- `src/Application/Features/Customers/Commands/CreateCustomer/` - Command pattern
- `src/Application/Features/Customers/Queries/GetCustomerById/` - Query pattern
- `src/Api/Controllers/V1/CustomersController.cs` - API endpoint pattern
- `tests/UnitTests/Domain/Customers/CustomerTests.cs` - Domain tests
- `tests/IntegrationTests/Api/Customers/CustomersControllerTests.cs` - API tests

---

### 🏗️ I'm Designing New Architecture

1. **Read**: [CLAUDE.md](CLAUDE.md) - Strict architecture rules
2. **Deep Dive**: [ARCHITECTURE.md](ARCHITECTURE.md) - Patterns & principles
3. **Study**: Current implementation of each layer
4. **Reference**: Best practices section

**Key Concepts**:
- Layered Architecture (Domain → Application → Infrastructure → API)
- Bounded Contexts (Auth, Customers)
- CQRS (Commands & Queries)
- Domain-Driven Design (Aggregates, Value Objects, Domain Events)
- Result<T> Pattern (no exceptions for business logic)

---

### 🔍 I'm Reviewing Code

1. **Reference**: [CLAUDE.md](CLAUDE.md) - Verify architecture rules & naming
2. **Check**: [ARCHITECTURE.md](ARCHITECTURE.md) - Best practices section
3. **Validate**:
   - Domain layer has zero dependencies ✓
   - Application depends only on Domain ✓
   - Commands & Queries have validators ✓
   - No exceptions for control flow ✓
   - Entities are sealed ✓
   - File-scoped namespaces ✓

---

### 🐛 I'm Debugging an Issue

1. **API Errors**: See [README.md](README.md) - Error Response format
2. **Test Failures**: Check [QUICKSTART.md](QUICKSTART.md) - Troubleshooting section
3. **Database Issues**: [QUICKSTART.md](QUICKSTART.md) - Database section
4. **Architecture Questions**: [ARCHITECTURE.md](ARCHITECTURE.md) - Data Flow section

---

### 📊 I'm Setting Up CI/CD or DevOps

Key Configuration Files:
- **[docker-compose.yml](docker-compose.yml)** - Local development environment
- **[src/Api/appsettings.json](src/Api/appsettings.json)** - Application settings
- **[.editorconfig](.editorconfig)** - Code formatting rules
- **[.gitignore](.gitignore)** - Git ignore patterns

---

## 📑 Complete Documentation Map

### 📋 Project Overview
| Document | Purpose | Length |
|----------|---------|--------|
| [README.md](README.md) | Features, quick start, API reference | 5-10 min |
| [QUICKSTART.md](QUICKSTART.md) | Step-by-step getting started guide | 10-15 min |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Deep dive into patterns & design | 20-30 min |
| [DOCS_INDEX.md](DOCS_INDEX.md) (this file) | Documentation navigation | 5 min |

### 🏗️ Architecture & Design
| Document | Purpose |
|----------|---------|
| [CLAUDE.md](CLAUDE.md) | Architecture rules, naming conventions, critical practices |
| [ARCHITECTURE.md](ARCHITECTURE.md) | DDD, CQRS, Result pattern, data flow explanation |

### ⚙️ Configuration
| File | Purpose |
|------|---------|
| [appsettings.json](src/Api/appsettings.json) | Database, JWT, logging configuration |
| [docker-compose.yml](docker-compose.yml) | PostgreSQL & API container setup |
| [.editorconfig](.editorconfig) | Code style & formatting rules |

### 📚 Reference
| Location | Content |
|----------|---------|
| [src/Domain/](src/Domain/) | Pure business logic (entities, value objects, events) |
| [src/Application/](src/Application/) | CQRS handlers, validators, use cases |
| [src/Infrastructure/](src/Infrastructure/) | Persistence, Identity, DI registration |
| [src/Api/](src/Api/) | REST controllers, middleware, configuration |
| [tests/](tests/) | Unit & Integration test examples |

---

## 🔗 Documentation Hierarchy

```
README.md (Start here - overview)
    ↓
QUICKSTART.md (How to run it)
    ↓
ARCHITECTURE.md (How it works)
    ↓
CLAUDE.md (How to build with it)
    ↓
Source code (Reference implementation)
```

---

## 🎯 Quick Navigation by Topic

### Authentication & Authorization
- **What it is**: [README.md - Security Best Practices](README.md#-security-best-practices)
- **How it works**: [ARCHITECTURE.md - Entity Lifecycle](ARCHITECTURE.md#entity-lifecycle)
- **How to implement**: [QUICKSTART.md - First API Test](QUICKSTART.md#first-api-test-)
- **Configuration**: [appsettings.json](src/Api/appsettings.json) - Jwt section
- **Code**: [AuthService.cs](src/Infrastructure/Identity/Services/AuthService.cs)

### Domain-Driven Design
- **Overview**: [ARCHITECTURE.md - Domain-Driven Design](ARCHITECTURE.md#domain-driven-design)
- **Aggregates**: [ARCHITECTURE.md - Aggregate Roots](ARCHITECTURE.md#1-aggregate-roots)
- **Entities**: [ARCHITECTURE.md - Entities](ARCHITECTURE.md#2-entities)
- **Value Objects**: [ARCHITECTURE.md - Value Objects](ARCHITECTURE.md#3-value-objects)
- **Domain Events**: [ARCHITECTURE.md - Domain Events](ARCHITECTURE.md#5-domain-events)
- **Example**: [Customer.cs](src/Domain/Customers/Entities/Customer.cs)

### CQRS Pattern
- **Overview**: [ARCHITECTURE.md - CQRS Pattern](ARCHITECTURE.md#cqrs-pattern)
- **Commands**: [ARCHITECTURE.md - Command Example](ARCHITECTURE.md#command-example-create-customer)
- **Queries**: [ARCHITECTURE.md - Query Example](ARCHITECTURE.md#query-example-get-customers)
- **Implementation**: MediatR with Handlers & Validators
- **Example**: [src/Application/Features/Customers/](src/Application/Features/Customers/)

### Result Pattern
- **Why use it**: [ARCHITECTURE.md - Result Pattern](ARCHITECTURE.md#result-pattern)
- **Implementation**: [ARCHITECTURE.md - Implementation](ARCHITECTURE.md#implementation)
- **Usage**: [ARCHITECTURE.md - Usage Pattern](ARCHITECTURE.md#usage-pattern)
- **Code**: [Result.cs](src/Domain/Common/Models/Result.cs)

### Database & Persistence
- **Schema**: [ApplicationDbContext.cs](src/Infrastructure/Persistence/ApplicationDbContext.cs)
- **Migrations**: [QUICKSTART.md - Database](QUICKSTART.md#database--entityframework-core)
- **Soft Delete**: [ARCHITECTURE.md - Soft Delete](ARCHITECTURE.md#soft-delete)
- **Auditing**: [ARCHITECTURE.md - Auditing](ARCHITECTURE.md#auditing)
- **Interceptors**: [src/Infrastructure/Persistence/Interceptors/](src/Infrastructure/Persistence/Interceptors/)

### API Endpoints
- **All endpoints**: [README.md - API Documentation](README.md#-api-documentation)
- **Auth endpoints**: [README.md - Authentication Endpoints](README.md#authentication-endpoints)
- **Customers endpoints**: [README.md - Customers Endpoints](README.md#customers-endpoints)
- **How to add new**: [QUICKSTART.md - Create a New Feature](QUICKSTART.md#1-create-a-new-feature-example-add-products)
- **Example**: [CustomersController.cs](src/Api/Controllers/V1/CustomersController.cs)

### Testing
- **Unit tests**: [tests/UnitTests/](tests/UnitTests/)
- **Integration tests**: [tests/IntegrationTests/](tests/IntegrationTests/)
- **How to run**: [QUICKSTART.md - Run Tests](QUICKSTART.md#run-tests-)
- **Guide**: [QUICKSTART.md - Development Workflow](QUICKSTART.md#development-workflow-)

### Code Quality
- **Formatting**: [QUICKSTART.md - Format Code](QUICKSTART.md#2-format-code)
- **Conventions**: [CLAUDE.md - Naming Conventions](CLAUDE.md#naming-conventions)
- **Best Practices**: [ARCHITECTURE.md - Best Practices](ARCHITECTURE.md#best-practices)
- **EditorConfig**: [.editorconfig](.editorconfig)

---

## 💡 Common Questions

### Q: Where do I put new business logic?
**A**: In the Domain layer (`src/Domain/YourContext/Entities/`). See [ARCHITECTURE.md - Domain Layer](ARCHITECTURE.md#1-domain-layer-srcdomain).

### Q: How do I handle validation?
**A**: Create a `Validator` extending `AbstractValidator<T>` for each Command/Query. See [CLAUDE.md - Validators](CLAUDE.md#naming-conventions).

### Q: What's the Result<T> pattern and why use it?
**A**: It's functional error handling without exceptions. See [ARCHITECTURE.md - Result Pattern](ARCHITECTURE.md#result-pattern).

### Q: How do I add a new API endpoint?
**A**:
1. Create Command/Query in Application
2. Create Handler & Validator
3. Create Controller action in Api
4. Follow the Customers example

See [QUICKSTART.md - Create a New Feature](QUICKSTART.md#1-create-a-new-feature-example-add-products).

### Q: How do database migrations work?
**A**: See [QUICKSTART.md - Add a Database Migration](QUICKSTART.md#add-a-database-migration).

### Q: How do I run tests?
**A**: See [QUICKSTART.md - Run Tests](QUICKSTART.md#run-tests-).

### Q: How is soft delete implemented?
**A**: See [ARCHITECTURE.md - Soft Delete](ARCHITECTURE.md#soft-delete).

### Q: What's a domain event and when do I use it?
**A**: See [ARCHITECTURE.md - Domain Events](ARCHITECTURE.md#5-domain-events).

---

## 📞 Getting Help

### Documentation Doesn't Help?
1. Check [CLAUDE.md](CLAUDE.md) for architecture rules
2. Look at similar implementation in `Customers` feature
3. Check test files for usage examples
4. Review git commit messages for context

### Report Issues
- Open GitHub issue with clear description
- Include error message, steps to reproduce
- Link to relevant documentation

### Suggest Improvements
- Create discussion on GitHub
- Propose changes with reasoning
- Submit pull requests with documentation updates

---

## 🎓 Learning Path

**Beginner** (Goal: Understand the basics)
1. [README.md](README.md) - 10 min
2. [QUICKSTART.md](QUICKSTART.md) - 15 min
3. Run the API locally
4. Test endpoints with curl

**Intermediate** (Goal: Add a simple feature)
1. [ARCHITECTURE.md - Domain Layer](ARCHITECTURE.md#1-domain-layer-srcdomain) - 10 min
2. [ARCHITECTURE.md - Application Layer](ARCHITECTURE.md#2-application-layer-srcapplication) - 10 min
3. Copy `Customers` feature structure
4. Create a simple `Products` feature
5. Write tests

**Advanced** (Goal: Understand all patterns & design)
1. [ARCHITECTURE.md](ARCHITECTURE.md) - 30 min
2. [CLAUDE.md](CLAUDE.md) - 20 min
3. Study all bounded contexts
4. Review test coverage
5. Optimize queries with Specification Pattern

**Expert** (Goal: Extend/modify architecture)
1. Deep study of all [CLAUDE.md](CLAUDE.md) rules
2. Review all layer implementations
3. Consider new bounded contexts
4. Design new cross-cutting concerns

---

## 📊 Documentation Stats

| Metric | Value |
|--------|-------|
| Total Markdown Docs | 4 |
| Total Lines of Documentation | 2,000+ |
| Code Examples | 50+ |
| API Endpoints Documented | 8 |
| Architecture Diagrams | 5+ |

---

## 📅 Last Updated

- **README.md**: 2026-03-16
- **QUICKSTART.md**: 2026-03-16
- **ARCHITECTURE.md**: 2026-03-16
- **CLAUDE.md**: 2026-03-16 (Project instructions)

**Keep documentation up-to-date!** When you change architecture, update docs.

---

## 🎯 Documentation Checklist

Before deploying to production:
- [ ] Read [README.md](README.md) completely
- [ ] Follow [QUICKSTART.md](QUICKSTART.md) setup
- [ ] Understand [ARCHITECTURE.md](ARCHITECTURE.md) patterns
- [ ] Review [CLAUDE.md](CLAUDE.md) rules
- [ ] Check code follows conventions
- [ ] Write unit & integration tests
- [ ] Run `dotnet format` for consistency
- [ ] Run `dotnet test` and verify coverage
- [ ] Review error handling (Result<T> pattern)
- [ ] Verify database migrations run successfully
- [ ] Test authentication flows
- [ ] Check API versioning

---

**Happy coding! 📚✨**

Remember: **Good documentation is a sign of good architecture. Invest in both.**
