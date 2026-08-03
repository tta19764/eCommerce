using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Infrastructure.Outbox;

/// <summary>
/// Provides helpers for collecting domain events into outbox messages.
/// </summary>
public static class OutboxDbContextExtensions
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    /// <summary>
    /// Collects domain events from tracked entities and stores them as outbox messages.
    /// </summary>
    /// <param name="dbContext">The EF Core DbContext that tracks domain entities.</param>
    /// <param name="outboxDbContext">The outbox DbContext contract implemented by the same context.</param>
    public static void AddDomainEventsAsOutboxMessages(
        this DbContext dbContext,
        IOutboxDbContext outboxDbContext)
    {
        var outboxMessages = dbContext.ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.GetDomainEvents();

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .Select(domainEvent => OutboxMessage.Create(
                domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings),
                DateTime.UtcNow))
            .ToArray();

        if (outboxMessages.Length > 0)
        {
            outboxDbContext.OutboxMessages.AddRange(outboxMessages);
        }
    }
}
