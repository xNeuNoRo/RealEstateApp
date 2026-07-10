namespace RealEstateApp.Application.Dtos.Auth.Requests;

/// <summary>
/// Credenciales de inicio de sesión. Soporta userName o email en el mismo campo.
/// </summary>
public sealed record LoginRequest(string UserNameOrEmail, string Password);
