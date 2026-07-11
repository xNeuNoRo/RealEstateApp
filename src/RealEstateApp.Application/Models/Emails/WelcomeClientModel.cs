namespace RealEstateApp.Application.Models.Emails;

/// <summary>
/// Modelo para el correo de bienvenida/activación de cliente.
/// </summary>
public sealed record WelcomeClientModel(string FullName, string ActivationLink) : IEmailModel;
