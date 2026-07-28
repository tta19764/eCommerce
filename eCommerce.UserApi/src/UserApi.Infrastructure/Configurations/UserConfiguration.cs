using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserApi.Domain.Users;

namespace UserApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for user profile persistence.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures the user aggregate mapping.
    /// </summary>
    /// <param name="builder">The user entity type builder.</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(user => user.FirstName, firstNameBuilder =>
        {
            firstNameBuilder.Property(firstName => firstName.Value)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(user => user.LastName, lastNameBuilder =>
        {
            lastNameBuilder.Property(lastName => lastName.Value)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(user => user.Email, emailBuilder =>
        {
            emailBuilder.Property(email => email.Value)
                .HasColumnName("Email")
                .HasMaxLength(320)
                .IsRequired();

            emailBuilder.HasIndex(email => email.Value)
                .IsUnique();
        });

        builder.Property(user => user.ImageId);

        builder.Ignore(user => user.FullName);
    }
}
