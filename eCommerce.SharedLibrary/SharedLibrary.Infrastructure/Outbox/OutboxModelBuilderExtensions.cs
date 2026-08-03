using Microsoft.EntityFrameworkCore;

namespace SharedLibrary.Infrastructure.Outbox;

/// <summary>
/// Provides EF Core model configuration for shared outbox tables.
/// </summary>
public static class OutboxModelBuilderExtensions
{
    /// <summary>
    /// Adds the default outbox message mapping to a service DbContext model.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    /// <returns>The same model builder so calls can be chained.</returns>
    public static ModelBuilder ApplyOutboxMessageConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(message => message.Id);

            builder.Property(message => message.Id)
                .ValueGeneratedNever();

            builder.Property(message => message.Type)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(message => message.Content)
                .IsRequired();

            builder.Property(message => message.OccurredOnUtc)
                .IsRequired();

            builder.Property(message => message.ProcessedOnUtc);

            builder.Property(message => message.Error);

            builder.HasIndex(message => new
            {
                message.ProcessedOnUtc,
                message.OccurredOnUtc
            });

            builder.ToTable("OutboxMessages");
        });

        return modelBuilder;
    }
}
