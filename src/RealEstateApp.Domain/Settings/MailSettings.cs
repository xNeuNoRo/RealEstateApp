namespace RealEstateApp.Domain.Settings;

/// <summary>
/// Configuración SMTP para el envío de correos electrónicos.
/// </summary>
public sealed class MailSettings
{
    public const string SectionName = "MailSettings";
    public required string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpUser { get; set; }
    public string? SmtpPass { get; set; }
    public required string EmailFrom { get; set; }
    public required string DisplayName { get; set; }
    public bool UseSsl { get; set; }

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
