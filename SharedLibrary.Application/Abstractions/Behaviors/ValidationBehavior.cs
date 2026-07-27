using FluentValidation;
using MediatR;
using SharedLibrary.Application.Exceptions;

namespace SharedLibrary.Application.Abstractions.Behaviors;

/// <summary>
/// Runs FluentValidation validators before a MediatR request reaches its handler.
/// </summary>
/// <typeparam name="TRequest">The request type handled by the pipeline.</typeparam>
/// <typeparam name="TResponse">The response type returned by the request handler.</typeparam>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Validates the request and invokes the next handler when validation succeeds.
    /// </summary>
    /// <param name="request">The request being validated.</param>
    /// <param name="next">The next handler delegate in the MediatR pipeline.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The response returned by the next handler.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validators fail.</exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        // Normalize FluentValidation failures into the shared application validation error contract.
        var validationErrors = validators
            .Select(validator => validator.Validate(context))
            .Where(validationResult => validationResult.Errors.Any())
            .SelectMany(validationResult => validationResult.Errors)
            .Select(validationFailure => new ValidationError(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage))
            .ToList();

        if (validationErrors.Any())
        {
            throw new Exceptions.ValidationException(validationErrors);
        }

        return await next(cancellationToken);
    }
}
