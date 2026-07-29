namespace NotificationApi.Domain.Notifications;

/// <summary>
/// Notification job processing status.
/// </summary>
public enum NotificationJobStatus
{
    /// <summary>
    /// Waiting for the background worker.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Currently being sent.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Sent successfully.
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// Permanently failed.
    /// </summary>
    Failed = 3
}
