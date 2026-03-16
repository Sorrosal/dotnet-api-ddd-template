---
description: Scaffold unit tests for domain entities, value objects, and application handlers using AAA pattern and Result assertions
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <FullClassName or relative path>
---

# Scaffold Unit Tests with AAA Pattern and Result<T>

Parse `$ARGUMENTS` to identify the class to test. Read that class first to understand its structure and Result<T> usage.

Follow patterns in `CLAUDE.md`, `PROJECT_STRUCTURE.md`, and `BEST_PRACTICES.md`.

---

## Test Types and Locations

| Target | Location | Framework | Approach |
|--------|----------|-----------|----------|
| Domain Entity / Aggregate | `tests/UnitTests/Domain/{BoundedContext}/{ClassName}Tests.cs` | xUnit + FluentAssertions | Test Create, state methods, events, Result handling |
| Value Object | `tests/UnitTests/Domain/{BoundedContext}/{ClassName}Tests.cs` | xUnit + FluentAssertions | Test validation, equality, Result errors |
| Command Handler | `tests/UnitTests/Application/{Feature}/Commands/{Handler}Tests.cs` | xUnit + NSubstitute + FluentAssertions | Mock repos, test Result success/failure |
| Query Handler | `tests/UnitTests/Application/{Feature}/Queries/{Handler}Tests.cs` | xUnit + NSubstitute + FluentAssertions | Mock repos, test Result responses |

---

## AAA Pattern: Arrange-Act-Assert

```csharp
[Fact]
public async Task Handle_Should_ReturnOrderId_When_ValidRequest()
{
    // Arrange: Setup test data, mocks, expected values
    var command = new CreateOrderCommand("Test Order", "Description");
    var mockRepository = Substitute.For<IOrderRepository>();
    var mockUnitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new CreateOrderCommandHandler(mockRepository, mockUnitOfWork);

    // Act: Execute the method being tested
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert: Verify the result
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
    await mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
}
```

---

## Domain Entity / Aggregate Tests

```csharp
public sealed class OrderTests
{
    [Fact]
    public void Create_Should_ReturnSuccess_When_ValidData()
    {
        // Arrange
        const string name = "Test Order";
        const string description = "Test Description";

        // Act
        var result = Order.Create(name, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.DomainEvents.Should().ContainSingle(e => e is OrderCreatedDomainEvent);
    }

    [Fact]
    public void Create_Should_ReturnFailure_When_NameEmpty()
    {
        // Act
        var result = Order.Create("", "Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidName");
    }

    [Fact]
    public void AddItem_Should_RaiseDomainEvent_When_ItemAdded()
    {
        // Arrange
        var order = Order.Create("Test", "Desc").Value;
        var productId = new ProductId(Guid.NewGuid());

        // Act
        var result = order.AddItem(productId, 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.DomainEvents.Should().ContainSingle(e => e is OrderItemAddedDomainEvent);
    }
}
```

---

## Value Object Tests

```csharp
public sealed class MoneyTests
{
    [Fact]
    public void Create_Should_ReturnSuccess_When_ValidData()
    {
        // Act
        var result = Money.Create(100m, "USD");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100m);
        result.Value.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_Should_ReturnFailure_When_NegativeAmount()
    {
        // Act
        var result = Money.Create(-50m, "USD");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.InvalidAmount");
    }

    [Fact]
    public void Equality_Should_BeValueBased()
    {
        // Act
        var money1 = Money.Create(100m, "USD").Value;
        var money2 = Money.Create(100m, "USD").Value;

        // Assert
        money1.Should().Be(money2);
    }

    [Theory]
    [InlineData(100, "USD")]
    [InlineData(50.25, "EUR")]
    [InlineData(999999.99, "GBP")]
    public void Create_Should_ReturnSuccess_When_ValidAmounts(decimal amount, string currency)
    {
        // Act
        var result = Money.Create(amount, currency);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
```

---

## Command Handler Tests (with Mocking)

```csharp
public sealed class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _mockRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _mockUnitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CreateOrderCommandHandler> _mockLogger = Substitute.For<ILogger<CreateOrderCommandHandler>>();
    private readonly CreateOrderCommandHandler _sut;

    public CreateOrderCommandHandlerTests()
    {
        _sut = new CreateOrderCommandHandler(_mockRepository, _mockUnitOfWork, _mockLogger);
    }

    [Fact]
    public async Task Handle_Should_ReturnOrderId_When_ValidRequest()
    {
        // Arrange
        var command = new CreateOrderCommand("Test Order", "Description");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _mockRepository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_InvalidName()
    {
        // Arrange
        var command = new CreateOrderCommand("", "Description");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidName");
        await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnConcurrencyError_When_ConcurrentModification()
    {
        // Arrange
        var command = new CreateOrderCommand("Test", "Desc");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency", []));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Concurrency.Conflict");
    }
}
```

---

## Query Handler Tests

```csharp
public sealed class GetOrderByIdQueryHandlerTests
{
    private readonly IOrderRepository _mockRepository = Substitute.For<IOrderRepository>();
    private readonly ILogger<GetOrderByIdQueryHandler> _mockLogger = Substitute.For<ILogger<GetOrderByIdQueryHandler>>();
    private readonly GetOrderByIdQueryHandler _sut;

    public GetOrderByIdQueryHandlerTests()
    {
        _sut = new GetOrderByIdQueryHandler(_mockRepository, _mockLogger);
    }

    [Fact]
    public async Task Handle_Should_ReturnResponse_When_OrderExists()
    {
        // Arrange
        var orderId = new OrderId(Guid.NewGuid());
        var order = Order.Create("Test", "Desc").Value;
        _mockRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(order);

        var query = new GetOrderByIdQuery(orderId.Value);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_OrderDoesNotExist()
    {
        // Arrange
        var orderId = new OrderId(Guid.NewGuid());
        _mockRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var query = new GetOrderByIdQuery(orderId.Value);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }
}
```

---

## Naming Convention

`{MethodName}_Should_{ExpectedBehavior}_When_{Condition}`

Examples:
- `Create_Should_ReturnOrderId_When_ValidRequest`
- `Handle_Should_ReturnNotFound_When_OrderDoesNotExist`
- `AddItem_Should_RaiseDomainEvent_When_ItemIsValid`
- `Create_Should_ReturnFailure_When_NameEmpty`

---

## Assertions for Result<T>

```csharp
// Success assertions
result.IsSuccess.Should().BeTrue();
result.Value.Should().NotBeNull();
result.Value.Should().Be(expected);

// Failure assertions
result.IsFailure.Should().BeTrue();
result.Error.Code.Should().Be("ExpectedCode");
result.Error.Message.Should().Contain("expected text");

// Event assertions
entity.DomainEvents.Should().ContainSingle();
entity.DomainEvents.Should().ContainSingle(e => e is OrderCreatedDomainEvent);
```

---

## Mocking with NSubstitute

```csharp
// Setup return value
_repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
    .Returns(order);

// Verify method was called
await _repository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

// Verify method was NOT called
await _repository.DidNotReceive().DeleteAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>());

// Throw exception
_unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
    .ThrowsAsync(new DbUpdateConcurrencyException("message", []));
```

---

## Guidelines

✅ Use AAA pattern: Arrange, Act, Assert
✅ Test Result<T>.IsSuccess and Result<T>.IsFailure branches
✅ Use FluentAssertions for readable assertions
✅ Use NSubstitute for mocking
✅ Name tests clearly following convention
✅ Test both happy path and error cases
✅ Verify domain events are raised
✅ Mock external dependencies (repo, UoW, logger)
✅ Use [Theory] for parameterized tests with multiple inputs
✅ Keep tests deterministic (no DateTime.Now, use fixed values)
❌ Don't test framework code (EF Core, MediatR)
❌ Don't make tests interdependent
❌ Don't test implementation details

---

**Run tests:**
```bash
dotnet test tests/UnitTests
```

**Reference:** `BEST_PRACTICES.md` - Result Pattern, Exception Handling
