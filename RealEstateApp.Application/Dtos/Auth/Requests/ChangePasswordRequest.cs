namespace RealEstateApp.Application.Dtos.Auth.Requests;

/// <summary>
/// Cambia la contraseña del usuario autenticado (requiere la actual).
/// </summary>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
