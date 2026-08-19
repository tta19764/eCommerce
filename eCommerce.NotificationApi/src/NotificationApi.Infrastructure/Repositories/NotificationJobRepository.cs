using Microsoft.EntityFrameworkCore;
using NotificationApi.Domain.Notifications;
using SharedLibrary.Infrastructure.Repositories;

namespace NotificationApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for notification jobs.
/// </summary>
/// <param name="dbContext">The notification database context.</param>
/// <remarks>
/// Due-job selection returns tracked pending entities in oldest-first order. It does not select jobs left in the
/// processing state by an interrupted worker.
/// </remarks>
public sealed class NotificationJobRepository(NotificationDbContext dbContext)
    : Repository<NotificationJob, NotificationDbContext>(dbContext), INotificationJobRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<NotificationJob>> GetDueJobsAsync(
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return await DbSet
            .Where(job =>
                job.Status == NotificationJobStatus.Pending &&
                (job.NextAttemptAtUtc == null || job.NextAttemptAtUtc <= utcNow))
            .OrderBy(job => job.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
