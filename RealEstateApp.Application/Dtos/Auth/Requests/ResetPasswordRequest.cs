namespace RealEstateApp.Application.Dtos.Auth.Requests;

/// <summary>
/// Restablece la contraseña de un usuario usando el token recibido por correo.
/// </summary>
public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword
);
