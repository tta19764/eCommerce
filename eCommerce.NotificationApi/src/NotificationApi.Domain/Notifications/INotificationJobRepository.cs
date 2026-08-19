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
    /// Gets tracked pending jobs that are ready for mutation through domain methods and processing.
    /// </summary>
    /// <param name="batchSize">The maximum number of jobs to load in oldest-first order.</param>
    /// <param name="utcNow">The current UTC clock value.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>Tracked pending jobs whose next-attempt time is absent or not later than <paramref name="utcNow"/>.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<IReadOnlyCollection<NotificationJob>> GetDueJobsAsync(
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken);
}
