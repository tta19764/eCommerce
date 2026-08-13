using MassTransit;
using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;

namespace SellerApi.Application.Sellers.GetPendingSellers;

/// <summary>Handles pending seller application queries.</summary>
public sealed class GetPendingSellersQueryHandler(
    ISellerRepository repository,
    IRequestClient<GetUserDetailsRequest> userClient)
    : IQueryHandler<GetPendingSellersQuery, PagedListResponse<PendingSellerApplicationResponse>>
{
    /// <inheritdoc />
    public async Task<Result<PagedListResponse<PendingSellerApplicationResponse>>> Handle(
        GetPendingSellersQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var applications = await repository.GetPendingApplicationsAsync(page, pageSize, cancellationToken);
        var totalCount = await repository.CountPendingApplicationsAsync(cancellationToken);

        var itemTasks = applications.Select(application => MapAsync(application, cancellationToken));
        var items = await Task.WhenAll(itemTasks);

        return Result.Success(new PagedListResponse<PendingSellerApplicationResponse>(
            items,
            page,
            pageSize,
            totalCount));
    }

    private async Task<PendingSellerApplicationResponse> MapAsync(
        PendingSellerApplication application,
        CancellationToken cancellationToken)
    {
        var userResponse = await userClient.GetResponse<GetUserDetailsResponse>(
            new GetUserDetailsRequest(application.Seller.OwnerUserId),
            cancellationToken);
        var user = userResponse.Message;
        var store = application.Store;

        return new PendingSellerApplicationResponse(
            application.Seller.Id,
            application.Seller.Status,
            new SellerApplicantResponse(user.UserId, user.FullName, user.Email, user.Found),
            new ProposedStoreResponse(
                store.Id,
                store.Slug,
                store.Name,
                store.Description,
                store.CountryCode,
                store.DefaultCurrency,
                store.LogoImageId,
                store.BannerImageId),
            application.Seller.CreatedOnUtc);
    }
}
