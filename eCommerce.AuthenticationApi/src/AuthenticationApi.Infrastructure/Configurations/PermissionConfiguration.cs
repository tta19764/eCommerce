using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for permissions.
/// </summary>
public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    /// <summary>
    /// Executes the Configure operation.
    /// </summary>
    /// <param name="builder">The builder value.</param>
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Id)
            .ValueGeneratedNever();

        builder.Property(permission => permission.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(permission => permission.Name)
            .IsUnique();

        builder.HasData(Permission.All);
    }
}
