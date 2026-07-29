using SharedLibrary.Domain.Abstractions;

namespace NotificationApi.Domain.Notifications;

/// <summary>
/// Database-backed notification work item processed by the background worker.
/// </summary>
public sealed class NotificationJob : Entity
{
    private NotificationJob()
    {
        Type = string.Empty;
        Recipient = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
        Payload = string.Empty;
    }

    private NotificationJob(
        Guid id,
        string type,
        string recipient,
        string subject,
        string body,
        string payload,
        DateTime createdAtUtc)
        : base(id)
    {
        Type = type;
        Recipient = recipient;
        Subject = subject;
        Body = body;
        Payload = payload;
        Status = NotificationJobStatus.Pending;
        Attempts = 0;
        MaxAttempts = 5;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Job category used by the processor.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Email recipient for this notification.
    /// </summary>
    public string Recipient { get; private set; }

    /// <summary>
    /// Email subject.
    /// </summary>
    public string Subject { get; private set; }

    /// <summary>
    /// Email body.
    /// </summary>
    public string Body { get; private set; }

    /// <summary>
    /// Serialized source payload retained for diagnostics and replay.
    /// </summary>
    public string Payload { get; private set; }

    /// <summary>
    /// Processing status.
    /// </summary>
    public NotificationJobStatus Status { get; private set; }

    /// <summary>
    /// Number of delivery attempts.
    /// </summary>
    public int Attempts { get; private set; }

    /// <summary>
    /// Maximum number of attempts before the job is permanently failed.
    /// </summary>
    public int MaxAttempts { get; private set; }

    /// <summary>
    /// Last delivery error, when any.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// UTC creation time.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// UTC completion time for successful jobs.
    /// </summary>
    public DateTime? ProcessedAtUtc { get; private set; }

    /// <summary>
    /// UTC time before which the job should not be retried.
    /// </summary>
    public DateTime? NextAttemptAtUtc { get; private set; }

    /// <summary>
    /// Creates an email notification job.
    /// </summary>
    public static NotificationJob CreateEmail(
        string recipient,
        string subject,
        string body,
        string payload,
        DateTime utcNow)
    {
        return new NotificationJob(
            Guid.NewGuid(),
            "email",
            recipient.Trim(),
            subject.Trim(),
            body.Trim(),
            payload,
            utcNow);
    }

    /// <summary>
    /// Moves the job into processing state.
    /// </summary>
    public void StartProcessing()
    {
        Attempts++;
        Status = NotificationJobStatus.Processing;
        LastError = null;
    }

    /// <summary>
    /// Marks the job as delivered.
    /// </summary>
    public void MarkSucceeded(DateTime utcNow)
    {
        Status = NotificationJobStatus.Succeeded;
        ProcessedAtUtc = utcNow;
        NextAttemptAtUtc = null;
        LastError = null;
    }

    /// <summary>
    /// Marks the job as failed or schedules another retry.
    /// </summary>
    public void MarkFailed(string error, TimeSpan retryDelay, DateTime utcNow)
    {
        LastError = error;

        if (Attempts >= MaxAttempts)
        {
            Status = NotificationJobStatus.Failed;
            NextAttemptAtUtc = null;
            return;
        }

        Status = NotificationJobStatus.Pending;
        NextAttemptAtUtc = utcNow.Add(retryDelay);
    }
}
