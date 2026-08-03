using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for account-role assignments.
/// </summary>
public sealed class AccountRoleConfiguration : IEntityTypeConfiguration<AccountRole>
{
    /// <summary>
    /// Executes the Configure operation.
    /// </summary>
    /// <param name="builder">The builder value.</param>
    public void Configure(EntityTypeBuilder<AccountRole> builder)
    {
        builder.HasKey(accountRole => accountRole.Id);

        builder.Property(accountRole => accountRole.Id)
            .ValueGeneratedNever();

        builder.HasIndex(accountRole => new { accountRole.AccountId, accountRole.RoleId })
            .IsUnique();

        builder.HasOne(accountRole => accountRole.Role)
            .WithMany()
            .HasForeignKey(accountRole => accountRole.RoleId);
    }
}

