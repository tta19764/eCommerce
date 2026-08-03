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
public sealed class SendOrderStatusChangedConsumer(
    INotificationJobRepository notificationJobRepository,
    IUnitOfWork unitOfWork,
    IEmailTemplateRenderer emailTemplateRenderer) : IConsumer<SendOrderStatusChangedRequest>
{
    /// <summary>
    /// Handles an order status email request and queues it for background delivery.
    /// </summary>
    /// <param name="context">The MassTransit consume context.</param>
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
