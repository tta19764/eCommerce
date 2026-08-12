using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers.SubmitSellerApplication;

/// <summary>Handles seller application submissions.</summary>
public sealed class SubmitSellerApplicationCommandHandler(
    ISellerRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<SubmitSellerApplicationCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(SubmitSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        if (await repository.GetByOwnerAsync(request.OwnerUserId, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(SellerApplicationErrors.AlreadyExists);
        }

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        if (await repository.GetStoreBySlugAsync(normalizedSlug, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(SellerApplicationErrors.SlugInUse);
        }

        var createdOnUtc = DateTime.UtcNow;
        var seller = Seller.Create(request.OwnerUserId, createdOnUtc);
        var storeResult = Store.Create(seller.Id, request.Slug, request.Name, request.Description, request.CountryCode, request.DefaultCurrency, createdOnUtc);
        if (storeResult.IsFailure)
        {
            return Result.Failure<Guid>(storeResult.Error);
        }

        repository.Add(seller);
        repository.Add(storeResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(seller.Id);
    }
}
