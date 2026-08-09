using FluentAssertions;
using NotificationApi.Domain.Notifications;
using Xunit;

namespace NotificationApi.Domain.UnitTests.Notifications;

public sealed class NotificationJobTests
{
    [Fact]
    public void CreateEmail_ShouldInitializePendingJobAndTrimEnvelopeFields()
    {
        var createdAtUtc = DateTime.UtcNow;

        var job = NotificationJob.CreateEmail(
            " user@example.com ", " Subject ", " Body ", "{}", createdAtUtc);

        job.Type.Should().Be("email");
        job.Recipient.Should().Be("user@example.com");
        job.Subject.Should().Be("Subject");
        job.Body.Should().Be("Body");
        job.Status.Should().Be(NotificationJobStatus.Pending);
        job.Attempts.Should().Be(0);
        job.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void StartProcessing_ShouldIncrementAttemptsAndClearPreviousError()
    {
        var job = CreateJob();
        job.StartProcessing();
        job.MarkFailed("temporary", TimeSpan.Zero, DateTime.UtcNow);

        job.StartProcessing();

        job.Attempts.Should().Be(2);
        job.Status.Should().Be(NotificationJobStatus.Processing);
        job.LastError.Should().BeNull();
    }

    [Fact]
    public void MarkSucceeded_ShouldCompleteJobAndClearRetryState()
    {
        var job = CreateJob();
        job.StartProcessing();
        var processedAtUtc = DateTime.UtcNow;

        job.MarkSucceeded(processedAtUtc);

        job.Status.Should().Be(NotificationJobStatus.Succeeded);
        job.ProcessedAtUtc.Should().Be(processedAtUtc);
        job.NextAttemptAtUtc.Should().BeNull();
        job.LastError.Should().BeNull();
    }

    [Fact]
    public void MarkFailed_ShouldScheduleRetryBeforeMaximumAttempts()
    {
        var job = CreateJob();
        job.StartProcessing();
        var failedAtUtc = DateTime.UtcNow;

        job.MarkFailed("smtp unavailable", TimeSpan.FromMinutes(2), failedAtUtc);

        job.Status.Should().Be(NotificationJobStatus.Pending);
        job.LastError.Should().Be("smtp unavailable");
        job.NextAttemptAtUtc.Should().Be(failedAtUtc.AddMinutes(2));
    }

    [Fact]
    public void MarkFailed_ShouldPermanentlyFailAtMaximumAttempts()
    {
        var job = CreateJob();

        for (var attempt = 0; attempt < job.MaxAttempts; attempt++)
        {
            job.StartProcessing();
        }

        job.MarkFailed("permanent", TimeSpan.FromMinutes(2), DateTime.UtcNow);

        job.Status.Should().Be(NotificationJobStatus.Failed);
        job.NextAttemptAtUtc.Should().BeNull();
    }

    private static NotificationJob CreateJob() => NotificationJob.CreateEmail(
        "user@example.com", "Subject", "Body", "{}", DateTime.UtcNow);
}
