namespace DotnetApiDddTemplate.Domain.Customers.Repositories;

/// <summary>
/// Repository interface for Customer aggregate root.
/// Defines contract for data persistence operations.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Get customer by ID.
    /// </summary>
    Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customer by email.
    /// </summary>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if customer with email exists (excluding soft deleted).
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add customer to repository.
    /// </summary>
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all customers (excluding soft deleted).
    /// </summary>
    Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
}
