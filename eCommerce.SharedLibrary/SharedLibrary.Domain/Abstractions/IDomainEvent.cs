using MediatR;

namespace SharedLibrary.Domain.Abstractions;

/// <summary>
/// Represents a notification raised by the domain model.
/// </summary>
public interface IDomainEvent : INotification
{
}
