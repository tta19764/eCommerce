using MassTransit;

namespace SharedLibrary.Testing.Messaging;

/// <summary>
/// Provides a minimal MassTransit response for tests that substitute request clients.
/// </summary>
/// <typeparam name="T">The response message type.</typeparam>
public sealed class TestResponse<T>(T message) : Response<T>
    where T : class
{
    /// <inheritdoc />
    public T Message { get; } = message;

    object Response.Message => Message;

    /// <inheritdoc />
    public Guid? MessageId => null;

    /// <inheritdoc />
    public Guid? RequestId => null;

    /// <inheritdoc />
    public Guid? CorrelationId => null;

    /// <inheritdoc />
    public Guid? ConversationId => null;

    /// <inheritdoc />
    public Guid? InitiatorId => null;

    /// <inheritdoc />
    public DateTime? ExpirationTime => null;

    /// <inheritdoc />
    public Uri? SourceAddress => null;

    /// <inheritdoc />
    public Uri? DestinationAddress => null;

    /// <inheritdoc />
    public Uri? ResponseAddress => null;

    /// <inheritdoc />
    public Uri? FaultAddress => null;

    /// <inheritdoc />
    public DateTime? SentTime => null;

    /// <inheritdoc />
    public Headers Headers => null!;

    /// <inheritdoc />
    public HostInfo Host => null!;
}
