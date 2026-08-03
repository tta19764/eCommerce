using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Orders;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure.Outbox;

namespace OrderApi.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for order persistence.
/// </summary>
public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options), IUnitOfWork, IOutboxDbContext
{
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
        modelBuilder.ApplyOutboxMessageConfiguration();

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Persists changes and stores raised domain events as outbox messages in the same transaction.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.AddDomainEventsAsOutboxMessages(this);

        return await base.SaveChangesAsync(cancellationToken);
    }
}
