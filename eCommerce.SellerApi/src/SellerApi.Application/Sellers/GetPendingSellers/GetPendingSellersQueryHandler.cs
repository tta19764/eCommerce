using MassTransit;
using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;

namespace SellerApi.Application.Sellers.GetPendingSellers;

/// <summary>Builds the administrator review page for pending seller applications.</summary>
/// <param name="repository">The repository that pages pending seller and proposed-store records.</param>
/// <param name="userClient">The UserApi client that resolves applicant profile data.</param>
/// <remarks>Applicant requests for a page run concurrently. A missing profile remains visible through its found flag.</remarks>
public sealed class GetPendingSellersQueryHandler(
    ISellerRepository repository,
    IRequestClient<GetUserDetailsRequest> userClient)
    : IQueryHandler<GetPendingSellersQuery, PagedListResponse<PendingSellerApplicationResponse>>
{
    /// <summary>Gets pending applications with proposed stores and applicant data.</summary>
    /// <param name="request">The requested page values.</param>
    /// <param name="cancellationToken">The token that cancels database and UserApi requests.</param>
    /// <returns>
    /// A successful paged result ordered by application creation time. Page numbers below one become one, and page
    /// size is clamped from 1 through 100.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <exception cref="RequestException">UserApi does not return an applicant response.</exception>
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

    /// <summary>Enriches one pending application with its UserApi applicant profile.</summary>
    /// <param name="application">The pending seller and proposed store read model.</param>
    /// <param name="cancellationToken">The token that cancels the UserApi request.</param>
    /// <returns>The administrator-facing application response.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <exception cref="RequestException">UserApi does not return an applicant response.</exception>
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
