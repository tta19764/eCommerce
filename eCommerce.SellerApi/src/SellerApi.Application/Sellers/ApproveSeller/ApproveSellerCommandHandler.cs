using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers.ApproveSeller;

/// <summary>Handles seller application approvals.</summary>
public sealed class ApproveSellerCommandHandler(ISellerRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<ApproveSellerCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ApproveSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await repository.GetByIdAsync(request.SellerId, cancellationToken);
        if (seller is null)
        {
            return Result.Failure(SellerApplicationErrors.NotFound);
        }

        var result = seller.Approve(request.AdminUserId, DateTime.UtcNow);
        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
