namespace DotnetApiDddTemplate.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation.
/// Manages transaction scope and coordinates database operations.
/// </summary>
public sealed class UnitOfWork(
    ApplicationDbContext context,
    ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Saving changes to database");
            var result = await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Successfully saved {ChangeCount} changes", result);
            return result;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update error");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while saving changes");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }
}
