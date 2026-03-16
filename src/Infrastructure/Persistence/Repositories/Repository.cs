namespace DotnetApiDddTemplate.Infrastructure.Persistence.Repositories;

/// <summary>
/// Base repository implementation.
/// Provides common CRUD operations for all aggregates.
/// </summary>
public abstract class Repository<TAggregate, TId>(
    ApplicationDbContext context,
    ILogger<Repository<TAggregate, TId>> logger) : IRepository<TAggregate, TId>
    where TAggregate : class
    where TId : struct
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly ILogger<Repository<TAggregate, TId>> Logger = logger;

    public virtual async Task<TAggregate?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Getting aggregate by ID: {Id}", id);
        return await Context.Set<TAggregate>().FindAsync(new object?[] { id }, cancellationToken);
    }

    public virtual async Task<List<TAggregate>> GetAsync(
        ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Getting aggregates with specification");
        return await specification.Query.ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Adding aggregate");
        await Context.Set<TAggregate>().AddAsync(aggregate, cancellationToken);
    }

    public virtual async Task UpdateAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Updating aggregate");
        Context.Set<TAggregate>().Update(aggregate);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Deleting aggregate");
        Context.Set<TAggregate>().Remove(aggregate);
        await Task.CompletedTask;
    }
}
