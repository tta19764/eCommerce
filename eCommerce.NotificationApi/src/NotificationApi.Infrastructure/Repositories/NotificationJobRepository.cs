using Microsoft.EntityFrameworkCore;
using NotificationApi.Domain.Notifications;
using SharedLibrary.Infrastructure.Repositories;

namespace NotificationApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for notification jobs.
/// </summary>
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
