namespace DotnetApiDddTemplate.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets auditing fields.
/// Sets CreatedBy, ModifiedBy, CreatedAtUtc, ModifiedAtUtc timestamps.
/// </summary>
public sealed class AuditableInterceptor(
    ICurrentUser currentUser,
    ILogger<AuditableInterceptor> logger) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var userId = currentUser.Id ?? "system";
        var utcNow = DateTime.UtcNow;

        var entries = eventData.Context.ChangeTracker.Entries()
            .Where(e =>
            {
                var entityType = e.Entity.GetType();
                return entityType.BaseType is not null &&
                       entityType.BaseType.IsGenericType &&
                       entityType.BaseType.GetGenericTypeDefinition() == typeof(AuditableEntity<>);
            })
            .ToList();

        foreach (var entry in entries)
        {
            var entity = entry.Entity;
            var createdByProperty = entity.GetType().GetProperty("CreatedBy");
            var createdAtUtcProperty = entity.GetType().GetProperty("CreatedAtUtc");
            var modifiedByProperty = entity.GetType().GetProperty("ModifiedBy");
            var modifiedAtUtcProperty = entity.GetType().GetProperty("ModifiedAtUtc");

            switch (entry.State)
            {
                case EntityState.Added:
                    createdByProperty?.SetValue(entity, userId);
                    createdAtUtcProperty?.SetValue(entity, utcNow);
                    logger.LogInformation(
                        "Setting created audit fields for {EntityType}",
                        entity.GetType().Name);
                    break;

                case EntityState.Modified:
                    modifiedByProperty?.SetValue(entity, userId);
                    modifiedAtUtcProperty?.SetValue(entity, utcNow);
                    logger.LogInformation(
                        "Setting modified audit fields for {EntityType}",
                        entity.GetType().Name);
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
