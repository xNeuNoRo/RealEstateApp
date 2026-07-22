namespace RealEstateApp.Application.Models.Emails;

/// <summary>
/// Modelo de datos para el correo de activación de cuenta de cliente.
/// </summary>
public record AccountActivationModel(
    string UserName,
    string ActivationUrl,
    string SupportEmail = "soporte@realestateapp.local"
) : IEmailModel;
