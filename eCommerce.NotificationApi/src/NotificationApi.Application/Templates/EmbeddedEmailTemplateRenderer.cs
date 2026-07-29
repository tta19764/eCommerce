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
