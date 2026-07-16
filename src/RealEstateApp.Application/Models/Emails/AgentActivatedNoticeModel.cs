namespace RealEstateApp.Application.Models.Emails;

/// <summary>
/// Modelo para el correo de notificación cuando un administrador activa la cuenta de un agente.
/// </summary>
public sealed record AgentActivatedNoticeModel(string FullName, string LoginUrl, int CurrentYear)
    : IEmailModel;
