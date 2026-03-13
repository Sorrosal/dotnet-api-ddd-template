---
description: Scaffold a new CQRS command with handler and FluentValidation validator
allowed-tools: Read, Write, Edit, Glob, Grep
argument-hint: <FeatureName> <CommandName> [ReturnType=Guid]
---

# Scaffold New CQRS Command

Parse `$ARGUMENTS` to extract `FeatureName`, `CommandName`, and optional `ReturnType` (defaults to `Guid`). If missing, ask the user.

Follow ALL patterns and conventions defined in the root `CLAUDE.md`. Read it first.

## Files to Create

### 1. Command Record
**Path:** `src/Application/Features/{FeatureName}/Commands/{CommandName}/{CommandName}Command.cs`
```csharp
namespace DotnetApiDddTemplate.Application.Features.{FeatureName}.Commands.{CommandName};

public sealed record {CommandName}Command(
    // Add properties based on context — ask user if unclear
) : IRequest<Result<{ReturnType}>>;
```

### 2. Command Handler
**Path:** `src/Application/Features/{FeatureName}/Commands/{CommandName}/{CommandName}CommandHandler.cs`
```csharp
namespace DotnetApiDddTemplate.Application.Features.{FeatureName}.Commands.{CommandName};

public sealed class {CommandName}CommandHandler(
    I{AggregateRepository} repository,
    IUnitOfWork unitOfWork) : IRequestHandler<{CommandName}Command, Result<{ReturnType}>>
{
    public async Task<Result<{ReturnType}>> Handle(
        {CommandName}Command request,
        CancellationToken cancellationToken)
    {
        // 1. Load/create domain entity
        // 2. Call domain method (returns Result)
        // 3. Persist via repository
        // 4. await unitOfWork.SaveChangesAsync(cancellationToken);
        // 5. Return Result<{ReturnType}>.Success(...)
    }
}
```

### 3. Command Validator
**Path:** `src/Application/Features/{FeatureName}/Commands/{CommandName}/{CommandName}CommandValidator.cs`
```csharp
namespace DotnetApiDddTemplate.Application.Features.{FeatureName}.Commands.{CommandName};

public sealed class {CommandName}CommandValidator : AbstractValidator<{CommandName}Command>
{
    public {CommandName}CommandValidator()
    {
        // Add validation rules for each property
        // RuleFor(x => x.Property).NotEmpty().MaximumLength(200);
    }
}
```

## Guidelines

- Use **primary constructors** for the handler.
- The handler MUST call `unitOfWork.SaveChangesAsync` — commands modify state.
- Domain logic belongs in the entity, NOT in the handler. Handler orchestrates.
- Return `Result<T>.Failure(error)` for domain errors — never throw exceptions.
- File-scoped namespaces in all files.
- Verify build: `dotnet build`.
