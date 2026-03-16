namespace DotnetApiDddTemplate.UnitTests.Domain.Customers;

/// <summary>
/// Unit tests for Customer aggregate root.
/// Tests creation, validation, updates, and domain events.
/// </summary>
public sealed class CustomerTests
{
    [Fact]
    public void Create_Should_ReturnSuccess_When_ValidData()
    {
        // Arrange
        const string name = "John Doe";
        const string email = "john@example.com";
        const string phoneNumber = "+1234567890";

        // Act
        var result = Customer.Create(name, email, phoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Email.Should().Be(email);
        result.Value.PhoneNumber.Should().Be(phoneNumber);
        result.Value.DomainEvents.Should().ContainSingle(e => e is CustomerCreatedDomainEvent);
    }

    [Fact]
    public void Create_Should_ReturnFailure_When_NameEmpty()
    {
        // Act
        var result = Customer.Create("", "john@example.com");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NameRequired");
    }

    [Fact]
    public void Create_Should_ReturnFailure_When_NameTooLong()
    {
        // Arrange
        var longName = new string('A', 201);

        // Act
        var result = Customer.Create(longName, "john@example.com");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NameTooLong");
    }

    [Fact]
    public void Create_Should_ReturnFailure_When_EmailEmpty()
    {
        // Act
        var result = Customer.Create("John Doe", "");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.EmailRequired");
    }

    [Fact]
    public void Create_Should_ReturnFailure_When_EmailInvalid()
    {
        // Act
        var result = Customer.Create("John Doe", "invalid-email");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.InvalidEmail");
    }

    [Fact]
    public void Update_Should_RaiseDomainEvent_When_Successful()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "john@example.com").Value;
        customer.ClearDomainEvents();

        // Act
        var result = customer.Update("Jane Doe", "jane@example.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.DomainEvents.Should().ContainSingle(e => e is CustomerUpdatedDomainEvent);
    }

    [Fact]
    public void Update_Should_ReturnFailure_When_InvalidEmail()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "john@example.com").Value;

        // Act
        var result = customer.Update("Jane Doe", "invalid");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.InvalidEmail");
    }

    [Fact]
    public void Delete_Should_SetDeletedAt()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "john@example.com").Value;
        customer.ClearDomainEvents();

        // Act
        var result = customer.Delete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsDeleted.Should().BeTrue();
        customer.DeletedAt.Should().NotBeNull();
        customer.DomainEvents.Should().ContainSingle(e => e is CustomerDeletedDomainEvent);
    }

    [Fact]
    public void Delete_Should_ReturnFailure_When_AlreadyDeleted()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "john@example.com").Value;
        customer.Delete();

        // Act
        var result = customer.Delete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.Deleted");
    }
}
