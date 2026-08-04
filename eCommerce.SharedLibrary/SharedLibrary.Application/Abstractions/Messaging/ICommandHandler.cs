using MediatR;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Application.Abstractions.Messaging;

/// <summary>
/// Handles a command that returns only success or failure.
/// </summary>
/// <typeparam name="TCommand">The command type handled by the handler.</typeparam>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Handles a command that returns a response payload on success.
/// </summary>
/// <typeparam name="TCommand">The command type handled by the handler.</typeparam>
/// <typeparam name="TResponse">The successful response payload type.</typeparam>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
