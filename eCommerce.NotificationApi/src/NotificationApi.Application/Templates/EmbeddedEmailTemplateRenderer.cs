using System.Net;
using System.Reflection;
using NotificationApi.Application.Abstractions;

namespace NotificationApi.Application.Templates;

/// <summary>
/// Renders embedded HTML email templates using simple named placeholders.
/// </summary>
public sealed class EmbeddedEmailTemplateRenderer : IEmailTemplateRenderer
{
    private const string EmailConfirmationResourceName =
        "NotificationApi.Application.Templates.EmailConfirmation.html";

    private const string OrderStatusChangedResourceName =
        "NotificationApi.Application.Templates.OrderStatusChanged.html";

    private const string ConversationMessageResourceName =
        "NotificationApi.Application.Templates.ConversationMessage.html";

    /// <inheritdoc />
    public string RenderEmailConfirmation(
        string firstName,
        string lastName,
        string confirmationUrl)
    {
        var fullName = $"{firstName} {lastName}".Trim();

        return RenderTemplate(
            EmailConfirmationResourceName,
            new Dictionary<string, string>
            {
                ["firstName"] = firstName,
                ["lastName"] = lastName,
                ["fullName"] = string.IsNullOrWhiteSpace(fullName) ? firstName : fullName,
                ["confirmationUrl"] = confirmationUrl,
                ["year"] = DateTime.UtcNow.Year.ToString()
            });
    }

    /// <inheritdoc />
    public string RenderOrderStatusChanged(
        string fullName,
        Guid orderId,
        string status,
        DateTime changedAtUtc)
    {
        return RenderTemplate(
            OrderStatusChangedResourceName,
            new Dictionary<string, string>
            {
                ["fullName"] = string.IsNullOrWhiteSpace(fullName) ? "Customer" : fullName,
                ["orderId"] = orderId.ToString(),
                ["status"] = status,
                ["changedAtUtc"] = changedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'"),
                ["year"] = DateTime.UtcNow.Year.ToString()
            });
    }

    /// <inheritdoc />
    public string RenderConversationMessage(
        string recipientFullName,
        string senderFullName,
        string messagePreview,
        DateTime sentAtUtc)
    {
        return RenderTemplate(
            ConversationMessageResourceName,
            new Dictionary<string, string>
            {
                ["recipientFullName"] = string.IsNullOrWhiteSpace(recipientFullName) ? "there" : recipientFullName,
                ["senderFullName"] = string.IsNullOrWhiteSpace(senderFullName) ? "A marketplace participant" : senderFullName,
                ["messagePreview"] = messagePreview,
                ["sentAtUtc"] = sentAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'"),
                ["year"] = DateTime.UtcNow.Year.ToString()
            });
    }

    private static string RenderTemplate(string resourceName, IReadOnlyDictionary<string, string> values)
    {
        var assembly = typeof(EmbeddedEmailTemplateRenderer).Assembly;

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Email template resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);

        var template = reader.ReadToEnd();

        foreach (var (key, value) in values)
        {
            template = template.Replace(
                "{{" + key + "}}",
                WebUtility.HtmlEncode(value),
                StringComparison.Ordinal);
        }

        return template;
    }
}
