---
description: Scaffold unit tests for a domain entity, value object, or application command/query handler
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <FullClassName or relative path>
---

# Scaffold Unit Tests

Parse `$ARGUMENTS` to identify the class to test. Read that class first to understand its structure.

Follow ALL patterns and conventions defined in the root `CLAUDE.md`. Read it first.

## Determine Test Type

Read the target class and determine what it is:

| Target type | Test location | Test approach |
|---|---|---|
| Domain entity / Aggregate | `tests/UnitTests/Domain/{BoundedContext}/{ClassName}Tests.cs` | Test `Create`, state methods, domain event raising |
| Value object | `tests/UnitTests/Domain/{BoundedContext}/{ClassName}Tests.cs` | Test `Create` validation, equality |
| Command handler | `tests/UnitTests/Application/{Feature}/Commands/{Handler}Tests.cs` | Mock repo + UoW with NSubstitute |
| Query handler | `tests/UnitTests/Application/{Feature}/Queries/{Handler}Tests.cs` | Mock repo with NSubstitute |

## Test File Structure

```csharp
namespace DotnetApiDddTemplate.UnitTests.{Layer}.{Path};

public sealed class {ClassName}Tests
{
    // For handlers: declare substitutes as fields
    private readonly I{Repository} _{repository} = Substitute.For<I{Repository}>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly {Handler} _sut;

    public {ClassName}Tests()
    {
        _sut = new {Handler}(_{repository}, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_Return{ReturnType}_When_ValidRequest()
    {
        // Arrange
        // Act
        // Assert — FluentAssertions
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EntityNotFound()
    {
        // Arrange — repository returns null
        // Act
        // Assert result.IsFailure.Should().BeTrue()
    }
}
```

## For Domain Entities / Aggregates

- Test `Create` with valid data → returns `Result.Success`, entity has correct state, domain event is raised.
- Test `Create` with each invalid input → returns `Result.Failure` with expected error.
- Test each state-changing method: valid transition and invalid transition.
- Verify domain events: `entity.DomainEvents.Should().ContainSingle(e => e is {EventName}DomainEvent)`.

## For Value Objects

- Test `Create` with valid input → success, value equals expected.
- Test `Create` with each invalid input → failure with expected error.
- Test equality: two instances with same values should be equal.

## For Command Handlers

- Mock repository with `NSubstitute` (`Substitute.For<IRepository>()`).
- Setup return values with `_repo.GetByIdAsync(Arg.Any<Id>()).Returns(entity)`.
- Verify `SaveChangesAsync` was called: `await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>())`.
- Test failure when entity not found: repository returns `null`, result is failure.

## Naming Convention

`{MethodName}_Should_{ExpectedBehavior}_When_{Condition}`

Examples:
- `Create_Should_ReturnOrderId_When_ValidRequest`
- `Handle_Should_ReturnNotFound_When_OrderDoesNotExist`
- `AddItem_Should_RaiseDomainEvent_When_ItemIsValid`

## Guidelines

- Use `[Fact]` for single cases, `[Theory]` with `[InlineData]` for parameterized cases.
- Use FluentAssertions: `.Should().Be(...)`, `.Should().BeTrue()`, `.Should().ContainSingle(...)`.
- Tests must be deterministic — no external dependencies, no clocks, no randomness (use fixed values or Bogus seeded).
- Verify build: `dotnet build` then `dotnet test tests/UnitTests`.
