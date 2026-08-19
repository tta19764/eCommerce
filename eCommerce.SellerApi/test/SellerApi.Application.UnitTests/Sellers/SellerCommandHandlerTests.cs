using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SellerApi.Application.Sellers;
using SellerApi.Application.Sellers.ApproveSeller;
using SellerApi.Application.Sellers.RejectSeller;
using SellerApi.Application.Sellers.SubmitSellerApplication;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.UnitTests.Sellers;

/// <summary>Verifies seller command handler behavior and persistence coordination.</summary>
public sealed class SellerCommandHandlerTests
{
    private readonly ISellerRepository _repository = Substitute.For<ISellerRepository>();
    private readonly IStoreRepository _storeRepository = Substitute.For<IStoreRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    /// <summary>Verifies that submission adds a pending seller and normalized store.</summary>
    /// <returns>A task that completes when the handler assertions finish.</returns>
    [Fact]
    public async Task Submit_ShouldAddPendingSellerAndNormalizedStore()
    {
        var ownerUserId = Guid.NewGuid();
        var handler = new SubmitSellerApplicationCommandHandler(
            _repository,
            _storeRepository,
            _unitOfWork,
            NullLogger<SubmitSellerApplicationCommandHandler>.Instance);

        var result = await handler.Handle(
            new SubmitSellerApplicationCommand(ownerUserId, "  sample-store  ", "  Sample Store  ", "  Description  ", " ua ", " uah "),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repository.Received(1).Add(Arg.Is<Seller>(seller =>
            seller.Id == result.Value && seller.OwnerUserId == ownerUserId && seller.Status == SellerStatus.PendingReview));
        _storeRepository.Received(1).Add(Arg.Is<Store>(store =>
            store.SellerId == result.Value && store.Slug == "sample-store" && store.Name == "Sample Store"
            && store.CountryCode == "UA" && store.DefaultCurrency == "UAH"));
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>Verifies that an existing owner prevents submission and persistence.</summary>
    /// <returns>A task that completes when the handler assertions finish.</returns>
    [Fact]
    public async Task Submit_ShouldRejectExistingOwnerWithoutSaving()
    {
        var ownerUserId = Guid.NewGuid();
        _repository.GetByOwnerAsync(ownerUserId, CancellationToken.None).Returns(Seller.Create(ownerUserId, DateTime.UtcNow));
        var handler = new SubmitSellerApplicationCommandHandler(
            _repository,
            _storeRepository,
            _unitOfWork,
            NullLogger<SubmitSellerApplicationCommandHandler>.Instance);

        var result = await handler.Handle(
            new SubmitSellerApplicationCommand(ownerUserId, "sample-store", "Sample Store", string.Empty, "UA", "UAH"),
            CancellationToken.None);

        result.Error.Should().Be(SellerErrors.AlreadyExists);
        _repository.DidNotReceive().Add(Arg.Any<Seller>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that approval activates and persists a pending seller.</summary>
    /// <returns>A task that completes when the handler assertions finish.</returns>
    [Fact]
    public async Task Approve_ShouldActivateSellerAndSave()
    {
        var seller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        var adminUserId = Guid.NewGuid();
        _repository.GetByIdAsync(seller.Id, CancellationToken.None).Returns(seller);
        var handler = new ApproveSellerCommandHandler(
            _repository,
            _unitOfWork,
            NullLogger<ApproveSellerCommandHandler>.Instance);

        var result = await handler.Handle(new ApproveSellerCommand(seller.Id, adminUserId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        seller.Status.Should().Be(SellerStatus.Active);
        seller.ReviewedByUserId.Should().Be(adminUserId);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>Verifies that rejection of an unknown seller does not persist changes.</summary>
    /// <returns>A task that completes when the handler assertions finish.</returns>
    [Fact]
    public async Task Reject_ShouldReturnNotFoundWithoutSaving()
    {
        var handler = new RejectSellerCommandHandler(
            _repository,
            _unitOfWork,
            NullLogger<RejectSellerCommandHandler>.Instance);

        var result = await handler.Handle(
            new RejectSellerCommand(Guid.NewGuid(), Guid.NewGuid(), "The application is incomplete."),
            CancellationToken.None);

        result.Error.Should().Be(SellerErrors.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
