namespace SharedLibrary.Infrastructure.Outbox;

/// <summary>
/// Durable representation of a domain event waiting to be published.
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = string.Empty;
        Content = string.Empty;
    }

    private OutboxMessage(Guid id, string type, string content, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    /// <summary>
    /// Gets the outbox message identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the serialized domain event type name.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the serialized domain event payload.
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Gets the UTC time when the domain event was captured.
    /// </summary>
    public DateTime OccurredOnUtc { get; private set; }

    /// <summary>
    /// Gets the UTC time when the message was processed.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; private set; }

    /// <summary>
    /// Gets the last processing error, when publication failed.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Creates a new outbox message from a serialized domain event.
    /// </summary>
    /// <param name="type">The domain event type name.</param>
    /// <param name="content">The serialized domain event payload.</param>
    /// <param name="occurredOnUtc">The UTC time when the event occurred.</param>
    /// <returns>The created outbox message.</returns>
    public static OutboxMessage Create(string type, string content, DateTime occurredOnUtc)
    {
        return new OutboxMessage(Guid.NewGuid(), type, content, occurredOnUtc);
    }

    /// <summary>
    /// Marks the outbox message as processed.
    /// </summary>
    /// <param name="processedOnUtc">The UTC processing time.</param>
    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    /// <summary>
    /// Stores the last processing failure for retry diagnostics.
    /// </summary>
    /// <param name="error">The processing error.</param>
    public void MarkFailed(string error)
    {
        Error = error;
    }
}
