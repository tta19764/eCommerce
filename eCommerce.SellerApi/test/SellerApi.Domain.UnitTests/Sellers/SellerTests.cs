using SellerApi.Domain.Sellers;

namespace SellerApi.Domain.UnitTests.Sellers;

/// <summary>Verifies seller review-state transitions.</summary>
public sealed class SellerTests
{
    /// <summary>Verifies that approval changes a pending seller to active.</summary>
    [Fact]
    public void Approve_ChangesPendingSellerToActive()
    {
        var seller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        var result = seller.Approve(Guid.NewGuid(), DateTime.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Equal(SellerStatus.Active, seller.Status);
    }

    /// <summary>Verifies that an approved seller cannot be reviewed again.</summary>
    [Fact]
    public void Approve_RejectsSecondReview()
    {
        var seller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        seller.Approve(Guid.NewGuid(), DateTime.UtcNow);
        Assert.True(seller.Approve(Guid.NewGuid(), DateTime.UtcNow).IsFailure);
    }
}
