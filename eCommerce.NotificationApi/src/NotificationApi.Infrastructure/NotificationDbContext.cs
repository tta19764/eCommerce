using Microsoft.EntityFrameworkCore;
using NotificationApi.Domain.Notifications;
using SharedLibrary.Domain.Abstractions;

namespace NotificationApi.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for notification jobs.
/// </summary>
public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Notification jobs table.
    /// </summary>
    public DbSet<NotificationJob> NotificationJobs { get; set; }

    /// <summary>
    /// Applies notification entity mappings.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
