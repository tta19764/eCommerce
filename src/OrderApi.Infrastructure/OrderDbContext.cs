using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderApi.Domain.Orders;
using OrderApi.Infrastructure.Outbox;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for order persistence.
/// </summary>
public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options), IUnitOfWork
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    /// <summary>
    /// Orders table.
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Order items table.
    /// </summary>
    public DbSet<OrderItem> OrderItems { get; set; }

    /// <summary>
    /// Durable domain-event messages pending background publication.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    /// <summary>
    /// Applies all entity configurations from the infrastructure assembly.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Persists changes and stores raised domain events as outbox messages in the same transaction.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddDomainEventsAsOutboxMessages();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddDomainEventsAsOutboxMessages()
    {
        var outboxMessages = ChangeTracker
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
            OutboxMessages.AddRange(outboxMessages);
        }
    }
}
