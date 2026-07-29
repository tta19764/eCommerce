using Microsoft.Extensions.Logging;
using NotificationApi.Application.Abstractions;
using NotificationApi.Domain.Notifications;
using SharedLibrary.Domain.Abstractions;

namespace NotificationApi.Application;

/// <summary>
/// Processes due notification jobs from persistent storage.
/// </summary>
public sealed class NotificationJobProcessor(
    INotificationJobRepository notificationJobRepository,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    ILogger<NotificationJobProcessor> logger)
{
    /// <summary>
    /// Processes a batch of jobs that are ready for delivery.
    /// </summary>
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
        notificationJobRepository.Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendAsync(job.Recipient, job.Subject, job.Body, cancellationToken);

            job.MarkSucceeded(DateTime.UtcNow);
            notificationJobRepository.Update(job);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Processed notification job {NotificationJobId}", job.Id);
        }
        catch (Exception exception)
        {
            var retryDelay = TimeSpan.FromSeconds(Math.Min(300, Math.Pow(2, job.Attempts) * 15));

            job.MarkFailed(exception.Message, retryDelay, DateTime.UtcNow);
            notificationJobRepository.Update(job);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                exception,
                "Notification job {NotificationJobId} failed on attempt {Attempt}",
                job.Id,
                job.Attempts);
        }
    }
}
