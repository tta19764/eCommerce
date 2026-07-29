namespace NotificationApi.Messages.Emails;

/// <summary>
/// Requests an email-confirmation notification for a newly registered account.
/// </summary>
/// <param name="AccountId">The authentication account identifier.</param>
/// <param name="Email">The recipient email address.</param>
/// <param name="FirstName">The recipient first name.</param>
/// <param name="LastName">The recipient last name.</param>
public sealed record SendEmailConfirmationRequest(
    Guid AccountId,
    string Email,
    string FirstName,
    string LastName);
