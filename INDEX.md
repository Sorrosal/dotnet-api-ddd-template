# 📚 Documentation Index

All documentation has been moved to the **[docs/](docs/)** folder.

## Quick Navigation

### 🚀 Getting Started
- **[README.md](docs/README.md)** - Project overview and quick start
- **[QUICKSTART.md](docs/QUICKSTART.md)** - Step-by-step setup guide (5-15 minutes)

### 🏗️ Architecture & Design
- **[ARCHITECTURE.md](docs/ARCHITECTURE.md)** - Deep dive into DDD, CQRS, patterns
- **[CLAUDE.md](docs/CLAUDE.md)** - Architecture rules and conventions
- **[PROJECT_STRUCTURE.md](docs/PROJECT_STRUCTURE.md)** - Project organization

### 📖 Reference & Best Practices
- **[DOCS_INDEX.md](docs/DOCS_INDEX.md)** - Complete documentation navigation hub
- **[BEST_PRACTICES.md](docs/BEST_PRACTICES.md)** - 14+ enterprise best practices

---

## 📍 Documentation Folder Structure

```
docs/
├── README.md                 ← Start here
├── QUICKSTART.md            ← Get running quickly
├── ARCHITECTURE.md          ← Understand the design
├── DOCS_INDEX.md            ← Navigate by role/topic
├── CLAUDE.md                ← Architecture rules
├── PROJECT_STRUCTURE.md     ← Project organization
└── BEST_PRACTICES.md        ← Enterprise patterns
```

---

## 🎯 By Your Role

### 👤 **I'm a Developer**
1. [QUICKSTART.md](docs/QUICKSTART.md) - Get the API running
2. [ARCHITECTURE.md](docs/ARCHITECTURE.md) - Understand the patterns
3. [CLAUDE.md](docs/CLAUDE.md) - Follow the conventions

### 🏗️ **I'm an Architect**
1. [ARCHITECTURE.md](docs/ARCHITECTURE.md) - System design
2. [CLAUDE.md](docs/CLAUDE.md) - Architecture rules
3. [PROJECT_STRUCTURE.md](docs/PROJECT_STRUCTURE.md) - Organization

### 🔍 **I'm a Code Reviewer**
1. [CLAUDE.md](docs/CLAUDE.md) - Rules to verify
2. [ARCHITECTURE.md](docs/ARCHITECTURE.md) - Best practices section
3. [BEST_PRACTICES.md](docs/BEST_PRACTICES.md) - Patterns implemented

### 📊 **I'm DevOps/Infrastructure**
1. [QUICKSTART.md](docs/QUICKSTART.md) - Docker setup
2. [README.md](docs/README.md) - Configuration section
3. [docker-compose.yml](docker-compose.yml) - Container setup

---

## 🚀 Quick Start (60 seconds)

```bash
# Option 1: Docker Compose (recommended)
docker compose up --build

# Option 2: Local development
dotnet restore
dotnet ef database update --project src/Infrastructure --startup-project src/Api
dotnet run --project src/Api
```

Then visit: https://localhost:5001/swagger

---

## 📚 All Documentation

| File | Purpose | Read Time |
|------|---------|-----------|
| [README.md](docs/README.md) | Overview, features, API reference | 5-10 min |
| [QUICKSTART.md](docs/QUICKSTART.md) | Getting started guide | 10-15 min |
| [ARCHITECTURE.md](docs/ARCHITECTURE.md) | Technical deep dive | 20-30 min |
| [DOCS_INDEX.md](docs/DOCS_INDEX.md) | Navigation hub | 5 min |
| [CLAUDE.md](docs/CLAUDE.md) | Architecture rules | 15 min |
| [PROJECT_STRUCTURE.md](docs/PROJECT_STRUCTURE.md) | Project organization | 10 min |
| [BEST_PRACTICES.md](docs/BEST_PRACTICES.md) | 14+ best practices | 20 min |

**Total Documentation: 2,000+ lines of guides, examples, and explanations**

---

For complete navigation help, see **[DOCS_INDEX.md](docs/DOCS_INDEX.md)**
