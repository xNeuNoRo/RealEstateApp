namespace RealEstateApp.Application.Dtos.Auth.Requests;

/// <summary>
/// Activa una cuenta de cliente mediante el token de confirmación de email
/// enviado por correo.
/// </summary>
public sealed record ActivateAccountRequest(string UserId, string Token);
