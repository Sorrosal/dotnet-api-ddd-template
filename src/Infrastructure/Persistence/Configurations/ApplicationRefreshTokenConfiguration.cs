namespace DotnetApiDddTemplate.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ApplicationRefreshToken entity.
/// Maps refresh token table with indexes and constraints.
/// </summary>
public sealed class ApplicationRefreshTokenConfiguration : IEntityTypeConfiguration<ApplicationRefreshToken>
{
    public void Configure(EntityTypeBuilder<ApplicationRefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => x.UserId);
    }
}
