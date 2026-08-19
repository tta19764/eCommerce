using Microsoft.Extensions.Logging;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers.SubmitSellerApplication;

/// <summary>Creates one pending seller and proposed store for an owner.</summary>
/// <param name="sellerRepository">The repository that checks owner uniqueness and tracks the seller.</param>
/// <param name="storeRepository">The repository that checks slug uniqueness and tracks the proposed store.</param>
/// <param name="unitOfWork">The unit of work that commits both records together.</param>
/// <param name="logger">The logger that records application submission outcomes.</param>
/// <remarks>
/// Owner and normalized slug checks provide domain errors before insertion. Database unique constraints remain the
/// concurrency guard, so a conflicting concurrent insert can still propagate a persistence exception.
/// </remarks>
public sealed class SubmitSellerApplicationCommandHandler(
    ISellerRepository sellerRepository,
    IStoreRepository storeRepository,
    IUnitOfWork unitOfWork,
    ILogger<SubmitSellerApplicationCommandHandler> logger) : ICommandHandler<SubmitSellerApplicationCommand, Guid>
{
    /// <summary>Submits a seller application when the owner and store slug are unused.</summary>
    /// <param name="request">The owner identifier and proposed store data.</param>
    /// <param name="cancellationToken">The token that cancels uniqueness checks and persistence.</param>
    /// <returns>The pending seller identifier, or an owner, slug, or store-validation failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<Guid>> Handle(SubmitSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Submitting seller application for owner {OwnerUserId}", request.OwnerUserId);

        if (await sellerRepository.GetByOwnerAsync(request.OwnerUserId, cancellationToken) is not null)
        {
            logger.LogWarning("Seller application already exists for owner {OwnerUserId}", request.OwnerUserId);
            return Result.Failure<Guid>(SellerErrors.AlreadyExists);
        }

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        if (await storeRepository.GetBySlugAsync(normalizedSlug, cancellationToken) is not null)
        {
            logger.LogWarning("Store slug {StoreSlug} is already in use", normalizedSlug);
            return Result.Failure<Guid>(StoreErrors.SlugInUse);
        }

        var createdOnUtc = DateTime.UtcNow;
        var seller = Seller.Create(request.OwnerUserId, createdOnUtc);
        var storeResult = Store.Create(seller.Id, request.Slug, request.Name, request.Description, request.CountryCode, request.DefaultCurrency, createdOnUtc);
        if (storeResult.IsFailure)
        {
            return Result.Failure<Guid>(storeResult.Error);
        }

        sellerRepository.Add(seller);
        storeRepository.Add(storeResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created pending seller {SellerId} for owner {OwnerUserId}", seller.Id, request.OwnerUserId);
        return Result.Success(seller.Id);
    }
}
