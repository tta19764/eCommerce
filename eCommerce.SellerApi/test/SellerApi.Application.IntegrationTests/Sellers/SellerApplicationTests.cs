using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SellerApi.Application.IntegrationTests.Infrastructure;
using SellerApi.Application.Sellers.ApproveSeller;
using SellerApi.Application.Sellers.GetOwnSeller;
using SellerApi.Application.Sellers.GetPendingSellers;
using SellerApi.Application.Sellers.SubmitSellerApplication;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;

namespace SellerApi.Application.IntegrationTests.Sellers;

/// <summary>Verifies seller application workflows against the SellerApi database.</summary>
public sealed class SellerApplicationTests : BaseIntegrationTest
{
    private readonly IntegrationTestWebAppFactory _factory;

    /// <summary>Creates the seller application integration tests.</summary>
    /// <param name="factory">The shared test application factory.</param>
    public SellerApplicationTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    /// <summary>Verifies that submission and approval persist a queryable active seller and store.</summary>
    /// <returns>A task that completes when the database assertions finish.</returns>
    [Fact]
    public async Task SubmitAndApprove_ShouldPersistQueryableSellerAndStore()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;
        var submitCommand = new SubmitSellerApplicationCommand(
            ownerId,
            $"store-{Guid.NewGuid():N}",
            "Test Store",
            "Description",
            "UA",
            "UAH");

        // Act
        var created = await Sender.Send(submitCommand, cancellationToken);
        await Sender.Send(
            new ApproveSellerCommand(created.Value, Guid.NewGuid()),
            cancellationToken);
        var queried = await Sender.Send(
            new GetOwnSellerQuery(ownerId, false),
            cancellationToken);

        // Assert
        created.IsSuccess.Should().BeTrue();
        queried.Value.Status.Should().Be(SellerStatus.Active);

        var store = await DbContext.Stores
            .AsNoTracking()
            .SingleAsync(
                store => store.SellerId == created.Value,
                cancellationToken);

        store.Name.Should().Be("Test Store");
    }

    /// <summary>Verifies that an administrator resolves the configured marketplace seller.</summary>
    /// <returns>A task that completes when the database assertions finish.</returns>
    [Fact]
    public async Task GetOwn_ShouldReturnMarketplaceSellerForAnyAdmin()
    {
        // Arrange
        var marketplaceOwnerId = Guid.NewGuid();
        var currentAdminUserId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;
        var marketplaceSeller = Seller.Create(marketplaceOwnerId, DateTime.UtcNow);
        marketplaceSeller.Approve(marketplaceOwnerId, DateTime.UtcNow);
        var marketplaceStore = Store.Create(
            marketplaceSeller.Id,
            "marketplace",
            "Marketplace",
            "Products sold by the marketplace.",
            "US",
            "USD",
            DateTime.UtcNow).Value;

        DbContext.Sellers.Add(marketplaceSeller);
        DbContext.Stores.Add(marketplaceStore);
        await DbContext.SaveChangesAsync(cancellationToken);

        // Act
        var result = await Sender.Send(
            new GetOwnSellerQuery(currentAdminUserId, true),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(marketplaceSeller.Id);
        result.Value.OwnerUserId.Should().Be(marketplaceOwnerId);
        result.Value.Status.Should().Be(SellerStatus.Active);
    }

    /// <summary>Verifies that the pending queue contains applicant, store, and paging data.</summary>
    /// <returns>A task that completes when the database assertions finish.</returns>
    [Fact]
    public async Task GetPending_ShouldReturnApplicantStoreAndPagingMetadata()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;
        _factory.AddUser(ownerId, "Pending Seller", "pending.seller@example.com");

        var created = await Sender.Send(
            new SubmitSellerApplicationCommand(
                ownerId,
                $"pending-{Guid.NewGuid():N}",
                "Pending Store",
                "Pending store description",
                "UA",
                "UAH"),
            cancellationToken);

        // Act
        var result = await Sender.Send(
            new GetPendingSellersQuery(1, 20),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalCount.Should().BeGreaterThanOrEqualTo(1);

        var application = result.Value.Items.Single(item => item.SellerId == created.Value);
        application.Status.Should().Be(SellerStatus.PendingReview);
        application.Applicant.FullName.Should().Be("Pending Seller");
        application.Applicant.Email.Should().Be("pending.seller@example.com");
        application.Applicant.Found.Should().BeTrue();
        application.Store.Name.Should().Be("Pending Store");
        application.Store.CountryCode.Should().Be("UA");
        application.Store.DefaultCurrency.Should().Be("UAH");
    }
}
