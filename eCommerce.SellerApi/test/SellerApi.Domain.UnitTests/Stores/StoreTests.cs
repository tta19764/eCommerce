using SellerApi.Domain.Stores;

namespace SellerApi.Domain.UnitTests.Stores;

public sealed class StoreTests
{
    [Fact]
    public void Ratings_UsePersistedSumAndCount()
    {
        var store = Store.Create(Guid.NewGuid(), "sample-store", "Sample Store", "Description", "UA", "UAH", DateTime.UtcNow).Value;
        store.AddRating(5);
        store.AddRating(4);
        Assert.Equal(4.5m, store.AverageRating);
        Assert.Equal(2, store.ReviewCount);
    }

    [Fact]
    public void Create_RejectsInvalidSlug()
    {
        Assert.True(Store.Create(Guid.NewGuid(), "Bad Slug", "Sample Store", "", "UA", "UAH", DateTime.UtcNow).IsFailure);
    }
}
