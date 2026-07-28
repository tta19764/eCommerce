using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for roles.
/// </summary>
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .ValueGeneratedNever();

        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.HasMany(role => role.Permissions)
            .WithOne()
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(role => role.Permissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(Role.All.Select(role => new
        {
            role.Id,
            role.Name
        }));
    }
}
