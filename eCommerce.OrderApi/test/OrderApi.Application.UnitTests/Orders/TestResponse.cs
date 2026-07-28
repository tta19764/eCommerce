using MassTransit;

namespace OrderApi.Application.UnitTests.Orders;

internal sealed class TestResponse<T>(T message) : Response<T>
    where T : class
{
    public T Message { get; } = message;

    object Response.Message => Message;

    public Guid? MessageId => null;

    public Guid? RequestId => null;

    public Guid? CorrelationId => null;

    public Guid? ConversationId => null;

    public Guid? InitiatorId => null;

    public DateTime? ExpirationTime => null;

    public Uri? SourceAddress => null;

    public Uri? DestinationAddress => null;

    public Uri? ResponseAddress => null;

    public Uri? FaultAddress => null;

    public DateTime? SentTime => null;

    public Headers Headers => null!;

    public HostInfo Host => null!;
}
