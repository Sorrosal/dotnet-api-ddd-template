namespace DotnetApiDddTemplate.UnitTests.Application.Customers.Queries;

/// <summary>
/// Unit tests for GetCustomerByIdQueryHandler.
/// Tests successful retrieval and not found scenarios.
/// </summary>
public sealed class GetCustomerByIdQueryHandlerTests
{
    private readonly ICustomerRepository _mockRepository = Substitute.For<ICustomerRepository>();
    private readonly ILogger<GetCustomerByIdQueryHandler> _mockLogger = Substitute.For<ILogger<GetCustomerByIdQueryHandler>>();
    private readonly GetCustomerByIdQueryHandler _sut;

    public GetCustomerByIdQueryHandlerTests()
    {
        _sut = new GetCustomerByIdQueryHandler(_mockRepository, _mockLogger);
    }

    [Fact]
    public async Task Handle_Should_ReturnResponse_When_CustomerExists()
    {
        // Arrange
        var customerId = new CustomerId(Guid.NewGuid());
        var customer = Customer.Create("John Doe", "john@example.com").Value;
        customer.GetType().GetProperty("Id")?.SetValue(customer, customerId);

        _mockRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(customer);

        var query = new GetCustomerByIdQuery(customerId.Value);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("John Doe");
        result.Value.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_CustomerDoesNotExist()
    {
        // Arrange
        var customerId = new CustomerId(Guid.NewGuid());
        _mockRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var query = new GetCustomerByIdQuery(customerId.Value);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }
}
