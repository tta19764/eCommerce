using MessagingApi.Domain.Conversations;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for marketplace messaging.
/// </summary>
public sealed class MessagingDbContext(DbContextOptions<MessagingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Conversations table.
    /// </summary>
    public DbSet<Conversation> Conversations { get; set; }

    /// <summary>
    /// Conversation messages table.
    /// </summary>
    public DbSet<ConversationMessage> ConversationMessages { get; set; }

    /// <summary>
    /// Applies entity configurations from this assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MessagingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

