using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderApi.Infrastructure.Outbox;

/// <summary>
/// EF Core mapping for order outbox messages.
/// </summary>
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
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
    }
}
