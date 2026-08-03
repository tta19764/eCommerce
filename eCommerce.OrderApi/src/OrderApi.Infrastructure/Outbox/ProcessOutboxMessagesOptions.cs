namespace OrderApi.Infrastructure.Outbox;

/// <summary>
/// Configuration values that control order outbox processing.
/// </summary>
internal sealed class ProcessOutboxMessagesOptions
{
    /// <summary>
    /// Configuration section used to bind this options object.
    /// </summary>
    public const string SectionName = "BackgroundJobs:ProcessOutboxMessages";

    /// <summary>
    /// Number of seconds between Quartz executions.
    /// </summary>
    public int IntervalSeconds { get; init; } = 5;

    /// <summary>
    /// Maximum number of outbox messages processed in one execution.
    /// </summary>
    public int PageSize { get; init; } = 20;
}
