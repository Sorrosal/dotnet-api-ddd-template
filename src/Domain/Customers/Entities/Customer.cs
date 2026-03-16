namespace DotnetApiDddTemplate.Domain.Customers.Entities;

/// <summary>
/// Customer aggregate root.
/// Represents a customer entity with soft delete, auditing, and optimistic concurrency control.
/// </summary>
public sealed class Customer : AuditableEntity<CustomerId>
{
    /// <summary>
    /// Customer's full name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Customer's email address.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Customer's phone number.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Customer's address.
    /// </summary>
    public string? Address { get; private set; }

    /// <summary>
    /// Customer's city.
    /// </summary>
    public string? City { get; private set; }

    /// <summary>
    /// Customer's country.
    /// </summary>
    public string? Country { get; private set; }

    /// <summary>
    /// EF Core constructor.
    /// </summary>
    private Customer() { }

    /// <summary>
    /// Factory method to create a new customer.
    /// Encapsulates validation and initialization.
    /// </summary>
    public static Result<Customer> Create(
        string name,
        string email,
        string? phoneNumber = null,
        string? address = null,
        string? city = null,
        string? country = null)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result<Customer>.Failure(CustomerErrors.NameRequired);

        if (name.Length > 200)
            return Result<Customer>.Failure(CustomerErrors.NameTooLong);

        // Validate email
        if (string.IsNullOrWhiteSpace(email))
            return Result<Customer>.Failure(CustomerErrors.EmailRequired);

        if (!email.Contains('@') || !email.Contains('.'))
            return Result<Customer>.Failure(CustomerErrors.InvalidEmail);

        var customer = new Customer
        {
            Id = new CustomerId(Guid.NewGuid()),
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber,
            Address = address,
            City = city,
            Country = country
        };

        customer.RaiseDomainEvent(new CustomerCreatedDomainEvent(
            customer.Id,
            customer.Name,
            customer.Email));

        return Result<Customer>.Success(customer);
    }

    /// <summary>
    /// Update customer details.
    /// </summary>
    public Result Update(
        string name,
        string email,
        string? phoneNumber = null,
        string? address = null,
        string? city = null,
        string? country = null)
    {
        // Validate new values
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(CustomerErrors.NameRequired);

        if (name.Length > 200)
            return Result.Failure(CustomerErrors.NameTooLong);

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(CustomerErrors.EmailRequired);

        if (!email.Contains('@') || !email.Contains('.'))
            return Result.Failure(CustomerErrors.InvalidEmail);

        // Update properties
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        Country = country;

        // Raise event
        RaiseDomainEvent(new CustomerUpdatedDomainEvent(
            Id,
            Name,
            Email));

        return Result.Success();
    }

    /// <summary>
    /// Soft delete the customer.
    /// </summary>
    public Result Delete()
    {
        if (IsDeleted)
            return Result.Failure(CustomerErrors.Deleted);

        DeletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CustomerDeletedDomainEvent(Id));

        return Result.Success();
    }
}
