using MediatR;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Application.Abstractions.Messaging;

/// <summary>
/// Handles a command that returns only success or failure.
/// </summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Handles a command that returns a response payload on success.
/// </summary>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}