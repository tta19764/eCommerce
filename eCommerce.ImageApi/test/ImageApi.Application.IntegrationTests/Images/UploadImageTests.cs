using FluentAssertions;
using ImageApi.Application.Images.UploadImage;
using ImageApi.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ImageApi.Application.IntegrationTests.Images;

public sealed class UploadImageTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Upload_ShouldPersistMetadataAfterStorageSucceeds()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var content = new MemoryStream([1, 2, 3]);
        var command = new UploadImageCommand(
            "photo.png",
            "image/png",
            content.Length,
            content);

        // Act
        var result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var image = await DbContext.Images
            .AsNoTracking()
            .SingleAsync(cancellationToken);

        image.FileName.Should().Be("photo.png");
    }
}
