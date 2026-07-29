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
}
