using MediatR;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Application.Abstractions.Messaging;

/// <summary>
/// Handles a query and returns a read model on success.
/// </summary>
/// <typeparam name="TQuery">The query type handled by the handler.</typeparam>
/// <typeparam name="TResponse">The successful response payload type.</typeparam>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
