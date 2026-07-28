using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for permissions.
/// </summary>
public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(permission => permission.Code, codeBuilder =>
        {
            codeBuilder.Property(code => code.Value)
                .HasColumnName("Code")
                .HasMaxLength(200)
                .IsRequired();

            codeBuilder.HasIndex(code => code.Value)
                .IsUnique();
        });

        builder.Property(permission => permission.Description)
            .HasMaxLength(500)
            .IsRequired();
    }
}

