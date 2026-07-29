using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationApi.Domain.Notifications;

namespace NotificationApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for notification job persistence.
/// </summary>
public sealed class NotificationJobConfiguration : IEntityTypeConfiguration<NotificationJob>
{
    /// <summary>
    /// Configures the notification job table.
    /// </summary>
    public void Configure(EntityTypeBuilder<NotificationJob> builder)
    {
        builder.HasKey(job => job.Id);

        builder.Property(job => job.Id)
            .ValueGeneratedNever();

        builder.Property(job => job.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(job => job.Recipient)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(job => job.Subject)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(job => job.Body)
            .IsRequired();

        builder.Property(job => job.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(job => job.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(job => job.LastError)
            .HasMaxLength(1000);

        builder.HasIndex(job => new { job.Status, job.NextAttemptAtUtc, job.CreatedAtUtc });
    }
}
