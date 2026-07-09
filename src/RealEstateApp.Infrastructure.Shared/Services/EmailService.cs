using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Models.Emails;

namespace RealEstateApp.Infrastructure.Shared.Services;

/// <summary>
/// Implementación stub de IEmailService. Registra el envío sin enviar correo real.
/// Se reemplazará por una implementación SMTP real cuando la WebApp lo requiera.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendEmailAsync<T>(
        string to,
        string subject,
        string templateName,
        T model,
        CancellationToken ct = default
    ) where T : IEmailModel
    {
        _logger.LogInformation(
            "[Email stub] Para: {To} | Asunto: {Subject} | Plantilla: {Template}",
            to,
            subject,
            templateName
        );
        return Task.FromResult(true);
    }
}
