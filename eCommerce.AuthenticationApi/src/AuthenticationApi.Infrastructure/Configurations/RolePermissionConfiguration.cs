using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for role-permission assignments.
/// </summary>
public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });

        builder.ToTable("RolePermission");

        builder.HasOne(rolePermission => rolePermission.Permission)
            .WithMany()
            .HasForeignKey(rolePermission => rolePermission.PermissionId);

        builder.HasData(Role.All.SelectMany(role =>
            role.Permissions.Select(rolePermission => new
            {
                rolePermission.RoleId,
                rolePermission.PermissionId
            })));
    }
}
