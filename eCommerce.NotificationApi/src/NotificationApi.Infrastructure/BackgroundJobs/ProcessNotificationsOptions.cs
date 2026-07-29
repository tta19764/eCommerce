namespace NotificationApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Configuration values that control notification job processing.
/// </summary>
internal sealed class ProcessNotificationsOptions
{
    /// <summary>
    /// Configuration section used to bind this options object.
    /// </summary>
    public const string SectionName = "BackgroundJobs:ProcessNotifications";

    /// <summary>
    /// Number of seconds between Quartz executions.
    /// </summary>
    public int IntervalSeconds { get; init; } = 5;

    /// <summary>
    /// Maximum number of notification jobs processed in one execution.
    /// </summary>
    public int PageSize { get; init; } = 20;
}
