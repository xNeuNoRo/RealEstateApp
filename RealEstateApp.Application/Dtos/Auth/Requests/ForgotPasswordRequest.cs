namespace RealEstateApp.Application.Dtos.Auth.Requests;

/// <summary>
/// Solicita un token de restablecimiento de contraseña enviándolo al correo.
/// </summary>
public sealed record ForgotPasswordRequest(string Email, string Origin);
