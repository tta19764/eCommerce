using Microsoft.Extensions.Logging;
using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers.RejectSeller;

/// <summary>Rejects a pending seller application.</summary>
/// <param name="repository">The repository that loads the tracked seller.</param>
/// <param name="unitOfWork">The unit of work that persists the rejection.</param>
/// <param name="logger">The logger that records rejection outcomes.</param>
public sealed class RejectSellerCommandHandler(
    ISellerRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<RejectSellerCommandHandler> logger)
    : ICommandHandler<RejectSellerCommand>
{
    /// <summary>Changes a pending seller to rejected and records the administrator and reason.</summary>
    /// <param name="request">The seller, administrator, and rejection reason.</param>
    /// <param name="cancellationToken">The token that cancels lookup and persistence.</param>
    /// <returns>A success result, or a not-found or invalid-state failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result> Handle(RejectSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await repository.GetByIdAsync(request.SellerId, cancellationToken);
        if (seller is null)
        {
            logger.LogWarning("Seller {SellerId} was not found for rejection", request.SellerId);
            return Result.Failure(SellerErrors.NotFound);
        }

        var result = seller.Reject(request.AdminUserId, request.Reason, DateTime.UtcNow);
        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Rejected seller {SellerId}", request.SellerId);
        }

        return result;
    }
}
