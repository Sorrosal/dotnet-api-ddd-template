namespace DotnetApiDddTemplate.UnitTests.Application.Customers.Commands;

/// <summary>
/// Unit tests for CreateCustomerCommandHandler.
/// Tests successful creation, validation failures, and duplicate email handling.
/// </summary>
public sealed class CreateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _mockRepository = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _mockUnitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CreateCustomerCommandHandler> _mockLogger = Substitute.For<ILogger<CreateCustomerCommandHandler>>();
    private readonly CreateCustomerCommandHandler _sut;

    public CreateCustomerCommandHandlerTests()
    {
        _sut = new CreateCustomerCommandHandler(_mockRepository, _mockUnitOfWork, _mockLogger);
    }

    [Fact]
    public async Task Handle_Should_ReturnCustomerId_When_ValidRequest()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john@example.com", "+1234567890");
        _mockRepository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _mockRepository.Received(1).AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EmailAlreadyExists()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john@example.com");
        _mockRepository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.AlreadyExists");
        await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_NameEmpty()
    {
        // Arrange
        var command = new CreateCustomerCommand("", "john@example.com");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NameRequired");
        await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_InvalidEmail()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "invalid");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.InvalidEmail");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ConcurrencyConflict()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john@example.com");
        _mockRepository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency", []));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Concurrency.Conflict");
    }
}
