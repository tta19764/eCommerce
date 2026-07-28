using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for authentication accounts.
/// </summary>
public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(account => account.FirstName, firstNameBuilder =>
        {
            firstNameBuilder.Property(firstName => firstName.Value)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(account => account.LastName, lastNameBuilder =>
        {
            lastNameBuilder.Property(lastName => lastName.Value)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(account => account.Email, emailBuilder =>
        {
            emailBuilder.Property(email => email.Value)
                .HasColumnName("Email")
                .HasMaxLength(320)
                .IsRequired();

            emailBuilder.HasIndex(email => email.Value)
                .IsUnique();
        });

        builder.Property(account => account.IdentityId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(account => account.IdentityId)
            .IsUnique();

        builder.Property(account => account.IsActive)
            .IsRequired();

        builder.Property(account => account.CreatedAtUtc)
            .IsRequired();

        builder.Property(account => account.DeletedAtUtc);

        builder.HasMany(account => account.Roles)
            .WithOne()
            .HasForeignKey(accountRole => accountRole.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(account => account.Roles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
