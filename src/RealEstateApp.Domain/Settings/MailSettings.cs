namespace RealEstateApp.Domain.Settings;

/// <summary>
/// Configuración SMTP para el envío de correos electrónicos.
/// </summary>
public sealed class MailSettings
{
    public const string SectionName = "MailSettings";
    public required string SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 587;
    public string SmtpUser { get; init; } = string.Empty;
    public string SmtpPass { get; init; } = string.Empty;
    public required string EmailFrom { get; init; }
    public required string DisplayName { get; init; }
    public bool UseSsl { get; init; } = true;

    /// <summary>
    /// Verifica si la configuración SMTP está completa y válida.
    /// </summary>
    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(EmailFrom)
        && !string.IsNullOrWhiteSpace(SmtpHost)
        && SmtpPort > 0
        && !string.IsNullOrWhiteSpace(SmtpUser)
        && !string.IsNullOrWhiteSpace(SmtpPass);
}
