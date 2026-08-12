using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers.RejectSeller;

/// <summary>Handles seller application rejections.</summary>
public sealed class RejectSellerCommandHandler(ISellerRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<RejectSellerCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(RejectSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await repository.GetByIdAsync(request.SellerId, cancellationToken);
        if (seller is null)
        {
            return Result.Failure(SellerApplicationErrors.NotFound);
        }

        var result = seller.Reject(request.AdminUserId, request.Reason, DateTime.UtcNow);
        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
