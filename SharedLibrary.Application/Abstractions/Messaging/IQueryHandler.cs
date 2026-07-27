using MediatR;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Application.Abstractions.Messaging;

/// <summary>
/// Handles a query and returns a read model on success.
/// </summary>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}