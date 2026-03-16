namespace DotnetApiDddTemplate.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface.
/// Manages transaction scope and coordinates domain event dispatch.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Save all changes to database.
    /// Domain events are dispatched before SaveChanges via interceptor.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
