using MediatR;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Application.Abstractions.Messaging;

/// <summary>
/// Represents an application request that changes state and returns only success or failure.
/// </summary>
public interface ICommand : IRequest<Result>, IBaseCommand
{
}

/// <summary>
/// Represents an application request that changes state and returns a response payload.
/// </summary>
/// <typeparam name="TResponse">The successful response payload type.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand
{
}

/// <summary>
/// Marker interface used to identify requests that belong to the command pipeline.
/// </summary>
public interface IBaseCommand
{
}
