namespace DotnetApiDddTemplate.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Customer aggregate root.
/// Configures table, keys, properties, soft delete, auditing, and query filters.
/// </summary>
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // Table
        builder.ToTable("Customers");

        // Primary key
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new CustomerId(value));

        // Properties
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.Country)
            .HasMaxLength(100);

        // Soft delete
        builder.Property(x => x.DeletedAt);

        // Auditing
        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

        builder.Property(x => x.ModifiedBy);

        builder.Property(x => x.ModifiedAtUtc);

        // Optimistic concurrency
        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("NOT(\"IsDeleted\")");

        // Global query filter: exclude soft-deleted
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
