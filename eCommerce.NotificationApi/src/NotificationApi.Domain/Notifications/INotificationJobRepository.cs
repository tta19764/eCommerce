namespace NotificationApi.Domain.Notifications;

/// <summary>
/// Persistence operations for background notification jobs.
/// </summary>
public interface INotificationJobRepository
{
    /// <summary>
    /// Adds a new notification job.
    /// </summary>
    /// <param name="job">The job to persist.</param>
    void Add(NotificationJob job);

    /// <summary>
    /// Marks an existing notification job as changed.
    /// </summary>
    /// <param name="job">The job to update.</param>
    void Update(NotificationJob job);

    /// <summary>
    /// Gets pending jobs that are ready to be processed.
    /// </summary>
    /// <param name="batchSize">The maximum number of jobs to load.</param>
    /// <param name="utcNow">The current UTC clock value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ready jobs.</returns>
    Task<IReadOnlyCollection<NotificationJob>> GetDueJobsAsync(
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken);
}
