using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Interfaces;

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

    public Task SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation(
            "[Email stub] Para: {To} | Asunto: {Subject} | Cuerpo: {Body}",
            to,
            subject,
            body
        );
        return Task.CompletedTask;
    }
}
