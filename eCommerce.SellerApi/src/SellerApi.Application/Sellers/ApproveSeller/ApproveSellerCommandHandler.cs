using Microsoft.Extensions.Logging;
using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers.ApproveSeller;

/// <summary>Approves a pending seller application.</summary>
/// <param name="repository">The repository that loads the tracked seller.</param>
/// <param name="unitOfWork">The unit of work that persists the approval.</param>
/// <param name="logger">The logger that records approval outcomes.</param>
public sealed class ApproveSellerCommandHandler(
    ISellerRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ApproveSellerCommandHandler> logger)
    : ICommandHandler<ApproveSellerCommand>
{
    /// <summary>Changes a pending seller to active and records the reviewing administrator.</summary>
    /// <param name="request">The seller and administrator UserApi identifiers.</param>
    /// <param name="cancellationToken">The token that cancels lookup and persistence.</param>
    /// <returns>A success result, or a not-found or invalid-state failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result> Handle(ApproveSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await repository.GetByIdAsync(request.SellerId, cancellationToken);
        if (seller is null)
        {
            logger.LogWarning("Seller {SellerId} was not found for approval", request.SellerId);
            return Result.Failure(SellerErrors.NotFound);
        }

        var result = seller.Approve(request.AdminUserId, DateTime.UtcNow);
        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Approved seller {SellerId}", request.SellerId);
        }

        return result;
    }
}
