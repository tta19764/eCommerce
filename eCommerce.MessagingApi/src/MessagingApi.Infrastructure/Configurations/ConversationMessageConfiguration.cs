using MessagingApi.Domain.Conversations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MessagingApi.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for conversation messages.
/// </summary>
internal sealed class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("ConversationMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.ConversationId).IsRequired();
        builder.Property(message => message.Body).HasMaxLength(4000).IsRequired();
        builder.Property(message => message.Type).IsRequired();
        builder.Property(message => message.CreatedAtUtc).IsRequired();

        builder.HasIndex(message => new { message.ConversationId, message.CreatedAtUtc });
    }
}

