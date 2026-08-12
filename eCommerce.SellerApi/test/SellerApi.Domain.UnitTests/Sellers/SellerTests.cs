using SellerApi.Domain.Sellers;

namespace SellerApi.Domain.UnitTests.Sellers;

public sealed class SellerTests
{
    [Fact]
    public void Approve_ChangesPendingSellerToActive()
    {
        var seller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        var result = seller.Approve(Guid.NewGuid(), DateTime.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Equal(SellerStatus.Active, seller.Status);
    }

    [Fact]
    public void Approve_RejectsSecondReview()
    {
        var seller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        seller.Approve(Guid.NewGuid(), DateTime.UtcNow);
        Assert.True(seller.Approve(Guid.NewGuid(), DateTime.UtcNow).IsFailure);
    }
}
