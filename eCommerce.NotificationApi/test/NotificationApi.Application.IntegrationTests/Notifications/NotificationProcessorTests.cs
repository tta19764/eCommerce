using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationApi.Application;
using NotificationApi.Application.IntegrationTests.Infrastructure;
using NotificationApi.Domain.Notifications;

namespace NotificationApi.Application.IntegrationTests.Notifications;

public sealed class NotificationProcessorTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ProcessDueJobs_ShouldPersistSucceededState()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var notificationJob = NotificationJob.CreateEmail(
            "user@test.local",
            "Subject",
            "Body",
            "{}",
            DateTime.UtcNow);

        DbContext.NotificationJobs.Add(notificationJob);
        await DbContext.SaveChangesAsync(cancellationToken);

        var processor = Services.GetRequiredService<NotificationJobProcessor>();

        // Act
        var processedJobCount = await processor.ProcessDueJobsAsync(10, cancellationToken);

        // Assert
        processedJobCount.Should().Be(1);

        DbContext.ChangeTracker.Clear();
        var persistedJob = await DbContext.NotificationJobs
            .AsNoTracking()
            .SingleAsync(cancellationToken);

        persistedJob.Status.Should().Be(NotificationJobStatus.Succeeded);
    }
}
