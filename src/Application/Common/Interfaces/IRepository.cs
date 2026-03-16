namespace DotnetApiDddTemplate.Application.Common.Interfaces;

/// <summary>
/// Base repository interface for all aggregates.
/// Provides CRUD operations and specification support.
/// </summary>
public interface IRepository<TAggregate, in TId>
    where TAggregate : class
    where TId : struct
{
    /// <summary>
    /// Get aggregate by ID.
    /// </summary>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all aggregates matching specification.
    /// </summary>
    Task<List<TAggregate>> GetAsync(
        ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add aggregate to repository.
    /// </summary>
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update aggregate.
    /// </summary>
    Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete aggregate.
    /// </summary>
    Task DeleteAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}
