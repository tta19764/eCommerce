using Microsoft.EntityFrameworkCore;

namespace SharedLibrary.Infrastructure.Outbox;

/// <summary>
/// Defines the persistence contract required by the shared outbox processor.
/// </summary>
public interface IOutboxDbContext
{
    /// <summary>
    /// Durable domain-event messages pending background publication.
    /// </summary>
    DbSet<OutboxMessage> OutboxMessages { get; set; }

    /// <summary>
    /// Persists pending outbox changes to the database.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
