using System.Text.Json;
using MassTransit;
using NotificationApi.Application.Abstractions;
using NotificationApi.Domain.Notifications;
using NotificationApi.Messages.Emails;
using SharedLibrary.Domain.Abstractions;

namespace NotificationApi.Application.Notifications.Messaging;

/// <summary>
/// Stores order status change email requests as durable background jobs.
/// </summary>
/// <param name="notificationJobRepository">The repository that tracks the new notification job.</param>
/// <param name="unitOfWork">The unit of work that persists the job.</param>
/// <param name="emailTemplateRenderer">The renderer that creates the HTML message body.</param>
/// <remarks>The consumer does not deduplicate requests. A redelivered request can create another email job.</remarks>
public sealed class SendOrderStatusChangedConsumer(
    INotificationJobRepository notificationJobRepository,
    IUnitOfWork unitOfWork,
    IEmailTemplateRenderer emailTemplateRenderer) : IConsumer<SendOrderStatusChangedRequest>
{
    /// <summary>
    /// Handles an order status email request and queues it for background delivery.
    /// </summary>
    /// <param name="context">The MassTransit consume context.</param>
    /// <returns>A task that completes after the notification job is committed.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
    public async Task Consume(ConsumeContext<SendOrderStatusChangedRequest> context)
    {
        var message = context.Message;
        var subject = $"Your eCommerce order is {message.Status}";
        var body = emailTemplateRenderer.RenderOrderStatusChanged(
            message.FullName,
            message.OrderId,
            message.Status,
            message.ChangedAtUtc);

        var job = NotificationJob.CreateEmail(
            message.Email,
            subject,
            body,
            JsonSerializer.Serialize(message),
            DateTime.UtcNow);

        notificationJobRepository.Add(job);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
