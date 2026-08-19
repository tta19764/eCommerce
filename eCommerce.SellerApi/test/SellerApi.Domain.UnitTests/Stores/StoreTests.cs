using SellerApi.Domain.Stores;

namespace SellerApi.Domain.UnitTests.Stores;

/// <summary>Verifies store creation and rating-summary rules.</summary>
public sealed class StoreTests
{
    /// <summary>Verifies that the average rating uses the persisted sum and count.</summary>
    [Fact]
    public void Ratings_UsePersistedSumAndCount()
    {
        var store = Store.Create(Guid.NewGuid(), "sample-store", "Sample Store", "Description", "UA", "UAH", DateTime.UtcNow).Value;
        store.AddRating(5);
        store.AddRating(4);
        Assert.Equal(4.5m, store.AverageRating);
        Assert.Equal(2, store.ReviewCount);
    }

    /// <summary>Verifies that store creation rejects an invalid slug.</summary>
    [Fact]
    public void Create_RejectsInvalidSlug()
    {
        Assert.True(Store.Create(Guid.NewGuid(), "Bad Slug", "Sample Store", "", "UA", "UAH", DateTime.UtcNow).IsFailure);
    }
}
