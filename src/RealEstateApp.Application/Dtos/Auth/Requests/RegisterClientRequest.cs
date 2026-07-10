using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Dtos.Auth.Requests;

/// <summary>
/// Formulario de registro público para rol Cliente.
/// </summary>
public sealed record RegisterClientRequest(
    string FirstName,
    string LastName,
    string Phone,
    IAppFile? PhotoFile,
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword
);
