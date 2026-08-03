namespace ImageApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Configuration values that control cleanup of uploaded images that were never attached.
/// </summary>
internal sealed class CleanupUnusedImagesOptions
{
    /// <summary>
    /// Configuration section used to bind this options object.
    /// </summary>
    public const string SectionName = "BackgroundJobs:CleanupUnusedImages";

    /// <summary>
    /// Number of seconds between Quartz executions.
    /// </summary>
    public int IntervalSeconds { get; init; } = 3600;

    /// <summary>
    /// Minimum temporary image age before it is eligible for cleanup.
    /// </summary>
    public int MinimumAgeMinutes { get; init; } = 60;

    /// <summary>
    /// Maximum number of unused images removed in one execution.
    /// </summary>
    public int PageSize { get; init; } = 100;
}
