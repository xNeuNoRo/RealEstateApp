using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Dtos.Auth.Requests;

/// <summary>
/// Formulario de registro público para rol Agente.
/// Un administrador debe activar la cuenta manualmente.
/// </summary>
public sealed record RegisterAgentRequest(
    string FirstName,
    string LastName,
    string Phone,
    IAppFile? PhotoFile,
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword
);
