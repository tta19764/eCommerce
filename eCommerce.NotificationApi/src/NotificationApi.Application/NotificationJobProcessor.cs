using Microsoft.Extensions.Logging;
using NotificationApi.Application.Abstractions;
using NotificationApi.Domain.Notifications;
using SharedLibrary.Domain.Abstractions;

namespace NotificationApi.Application;

/// <summary>
/// Processes due notification jobs from persistent storage.
/// </summary>
/// <param name="notificationJobRepository">The repository that selects due jobs.</param>
/// <param name="emailSender">The delivery adapter that sends queued email.</param>
/// <param name="unitOfWork">The unit of work that persists each job state transition.</param>
/// <param name="logger">The logger that records delivery outcomes.</param>
/// <remarks>
/// The processor persists the <see cref="NotificationJobStatus.Processing"/> state before it calls the sender.
/// It then commits either success or a retry state. These commits do not form one transaction with SMTP delivery.
/// A process stop after the first commit can leave a job in the processing state; the current due-job query does
/// not recover such jobs.
/// </remarks>
public sealed class NotificationJobProcessor(
    INotificationJobRepository notificationJobRepository,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    ILogger<NotificationJobProcessor> logger)
{
    /// <summary>
    /// Processes a batch of jobs that are ready for delivery.
    /// </summary>
    /// <param name="batchSize">The maximum number of pending jobs to select, ordered from oldest to newest.</param>
    /// <param name="cancellationToken">The token that cancels repository, persistence, and email operations.</param>
    /// <returns>The number of jobs selected for processing, including jobs that failed and were rescheduled.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>
    /// Jobs are processed in sequence. Delivery failures use exponential delays of 30, 60, 120, and 240 seconds.
    /// The fifth failed attempt changes the job to its terminal failed state.
    /// </remarks>
    public async Task<int> ProcessDueJobsAsync(int batchSize, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var jobs = await notificationJobRepository.GetDueJobsAsync(batchSize, utcNow, cancellationToken);

        foreach (var job in jobs)
        {
            await ProcessJobAsync(job, cancellationToken);
        }

        return jobs.Count;
    }

    private async Task ProcessJobAsync(NotificationJob job, CancellationToken cancellationToken)
    {
        job.StartProcessing();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendAsync(job.Recipient, job.Subject, job.Body, cancellationToken);

            job.MarkSucceeded(DateTime.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Processed notification job {NotificationJobId}", job.Id);
        }
        catch (Exception exception)
        {
            // A capped exponential delay reduces SMTP load while keeping early transient failures responsive.
            var retryDelay = TimeSpan.FromSeconds(Math.Min(300, Math.Pow(2, job.Attempts) * 15));

            job.MarkFailed(exception.Message, retryDelay, DateTime.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                exception,
                "Notification job {NotificationJobId} failed on attempt {Attempt}",
                job.Id,
                job.Attempts);
        }
    }
}
