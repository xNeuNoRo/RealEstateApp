namespace RealEstateApp.Application.Dtos.Auth.Requests;

/// <summary>
/// Solicita el reenvío del correo de activación para una cuenta de cliente.
/// </summary>
public sealed record ResendActivationRequest(string Email, string Origin);
