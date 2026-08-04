namespace NotificationApi.Application.Abstractions;

/// <summary>
/// Renders HTML email templates.
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Renders the email confirmation template.
    /// </summary>
    /// <param name="firstName">The recipient first name.</param>
    /// <param name="lastName">The recipient last name.</param>
    /// <param name="confirmationUrl">The confirmation link.</param>
    /// <returns>The rendered HTML body.</returns>
    string RenderEmailConfirmation(
        string firstName,
        string lastName,
        string confirmationUrl);

    /// <summary>
    /// Renders the order status change template.
    /// </summary>
    /// <param name="fullName">The recipient display name.</param>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="status">The new order status.</param>
    /// <param name="changedAtUtc">The UTC status change timestamp.</param>
    /// <returns>The rendered HTML body.</returns>
    string RenderOrderStatusChanged(
        string fullName,
        Guid orderId,
        string status,
        DateTime changedAtUtc);

    /// <summary>
    /// Renders the marketplace conversation message template.
    /// </summary>
    /// <param name="recipientFullName">The recipient display name.</param>
    /// <param name="senderFullName">The sender display name.</param>
    /// <param name="messagePreview">A short preview of the message body.</param>
    /// <param name="sentAtUtc">The UTC timestamp when the message was sent.</param>
    /// <returns>The rendered HTML body.</returns>
    string RenderConversationMessage(
        string recipientFullName,
        string senderFullName,
        string messagePreview,
        DateTime sentAtUtc);
}
