using RealEstateApp.Application.Models.Emails;

namespace RealEstateApp.Application.Interfaces;

/// <summary>
/// Servicio de envío de correos electrónicos con plantillas Razor.
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync<T>(
        string to,
        string subject,
        string templateName,
        T model,
        CancellationToken ct = default
    ) where T : IEmailModel;
}
