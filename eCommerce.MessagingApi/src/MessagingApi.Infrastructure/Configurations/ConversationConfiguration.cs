using MessagingApi.Domain.Conversations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MessagingApi.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for conversations.
/// </summary>
internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(conversation => conversation.Id);

        builder.Property(conversation => conversation.Type).IsRequired();
        builder.Property(conversation => conversation.CustomerUserId).IsRequired();
        builder.Property(conversation => conversation.SellerUserId).IsRequired();
        builder.Property(conversation => conversation.Status).IsRequired();
        builder.Property(conversation => conversation.CreatedAtUtc).IsRequired();
        builder.Property(conversation => conversation.LastMessageAtUtc).IsRequired();

        builder
            .HasMany(conversation => conversation.Messages)
            .WithOne()
            .HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(conversation => conversation.Messages)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(conversation => new { conversation.CustomerUserId, conversation.LastMessageAtUtc });
        builder.HasIndex(conversation => new { conversation.SellerUserId, conversation.LastMessageAtUtc });
        builder.HasIndex(conversation => new { conversation.CustomerUserId, conversation.SellerUserId, conversation.ProductId })
            .IsUnique()
            .HasFilter("\"ProductId\" IS NOT NULL");
        builder.HasIndex(conversation => conversation.SellerOrderId)
            .IsUnique()
            .HasFilter("\"SellerOrderId\" IS NOT NULL");
    }
}

