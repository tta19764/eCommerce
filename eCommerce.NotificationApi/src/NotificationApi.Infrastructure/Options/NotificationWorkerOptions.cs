namespace NotificationApi.Infrastructure.Options;

/// <summary>
/// Background notification worker settings.
/// </summary>
public sealed class NotificationWorkerOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "NotificationWorker";

    /// <summary>
    /// Delay between polling attempts.
    /// </summary>
    public int PollingIntervalSeconds { get; init; } = 5;

    /// <summary>
    /// Maximum jobs processed per batch.
    /// </summary>
    public int BatchSize { get; init; } = 20;
}
