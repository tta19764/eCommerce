namespace NotificationApi.Messages.Emails;

/// <summary>
/// Requests an order status change notification email.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Email">The recipient email address.</param>
/// <param name="FullName">The recipient display name.</param>
/// <param name="Status">The new order status.</param>
/// <param name="ChangedAtUtc">The UTC time when the status changed.</param>
public sealed record SendOrderStatusChangedRequest(
    Guid OrderId,
    string Email,
    string FullName,
    string Status,
    DateTime ChangedAtUtc);
