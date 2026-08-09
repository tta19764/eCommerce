using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationApi.Application;
using NotificationApi.Application.Abstractions;
using NotificationApi.Domain.Notifications;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace NotificationApi.Application.UnitTests;

public sealed class NotificationJobProcessorTests
{
    private readonly INotificationJobRepository _repository = Substitute.For<INotificationJobRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task ProcessDueJobsAsync_ShouldSendAndCompleteDueJob()
    {
        var cancellationToken = CancellationToken.None;
        var job = CreateJob();
        _repository.GetDueJobsAsync(10, Arg.Any<DateTime>(), cancellationToken)
            .Returns([job]);
        var processor = CreateProcessor();

        var processedCount = await processor.ProcessDueJobsAsync(10, cancellationToken);

        processedCount.Should().Be(1);
        job.Status.Should().Be(NotificationJobStatus.Succeeded);
        job.Attempts.Should().Be(1);
        await _emailSender.Received(1).SendAsync(
            job.Recipient, job.Subject, job.Body, cancellationToken);
        await _unitOfWork.Received(2).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task ProcessDueJobsAsync_ShouldScheduleRetry_WhenSendingFails()
    {
        var cancellationToken = CancellationToken.None;
        var job = CreateJob();
        _repository.GetDueJobsAsync(10, Arg.Any<DateTime>(), cancellationToken)
            .Returns([job]);
        _emailSender.SendAsync(job.Recipient, job.Subject, job.Body, cancellationToken)
            .Returns(Task.FromException(new InvalidOperationException("smtp unavailable")));
        var processor = CreateProcessor();

        var processedCount = await processor.ProcessDueJobsAsync(10, cancellationToken);

        processedCount.Should().Be(1);
        job.Status.Should().Be(NotificationJobStatus.Pending);
        job.LastError.Should().Be("smtp unavailable");
        job.NextAttemptAtUtc.Should().NotBeNull();
        await _unitOfWork.Received(2).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task ProcessDueJobsAsync_ShouldReturnZero_WhenNoJobsAreDue()
    {
        var cancellationToken = CancellationToken.None;
        _repository.GetDueJobsAsync(10, Arg.Any<DateTime>(), cancellationToken)
            .Returns(Array.Empty<NotificationJob>());

        var processedCount = await CreateProcessor().ProcessDueJobsAsync(10, cancellationToken);

        processedCount.Should().Be(0);
        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private NotificationJobProcessor CreateProcessor() => new(
        _repository,
        _emailSender,
        _unitOfWork,
        NullLogger<NotificationJobProcessor>.Instance);

    private static NotificationJob CreateJob() => NotificationJob.CreateEmail(
        "user@example.com", "Subject", "Body", "{}", DateTime.UtcNow);
}
